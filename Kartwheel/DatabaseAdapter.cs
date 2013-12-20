using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.Reflection;


namespace Kartwheel
{
	class DatabaseAdapter
	{
		private DataGridView control;
		private SqlCeConnection connection;
		private string cString = @"Data Source=Database.sdf";
		public DatabaseAdapter(DataGridView inputControl)
		{
			connection = new System.Data.SqlServerCe.SqlCeConnection(cString);
			control = inputControl;
		}

		public DatabaseAdapter() 
		{
			connection = new System.Data.SqlServerCe.SqlCeConnection(cString);
		}

		public void SaveData(DataGridView inputDataGridView, string table)
		{
			string className = getClassName(table);
			List<object> arr = new List<object>();

			for (int i = 0; i < inputDataGridView.Rows.Count - 1; i++)
			{
				object[] param = new object[inputDataGridView.Columns.Count];
				for (int j = 0; j < inputDataGridView.Columns.Count; j++)
				{
					param[j] = inputDataGridView.Rows[i].Cells[j].Value;
				}
				object item = Activator.CreateInstance(Type.GetType(className), param);
				arr.Add(item);				
			}

			connection.Open();
			SqlCeCommand command = new System.Data.SqlServerCe.SqlCeCommand("DELETE [" + table + "]", connection);
			command.ExecuteNonQuery();

			for (int i = 0; i < arr.Count; i++)
			{
				string query = "INSERT INTO [" + table + "] (";
				foreach (PropertyInfo p in arr[i].GetType().GetProperties())
				{
					query += "" + p.ToString().Substring(p.ToString().IndexOf(' ')).ToLower() + ", ";
				}
				query = query.Remove(query.Length - 2) + ") VALUES (";				
				foreach (PropertyInfo p in arr[i].GetType().GetProperties())
				{
					query += "'" + p.GetValue(arr[i], null).ToString() + "', ";
				}
				query = query.Remove(query.Length - 2) + ")";
				SqlCeCommand insertCommand = new SqlCeCommand(query, connection);
				insertCommand.ExecuteNonQuery();
			}

			connection.Close();
		}

		public DataGridView FillTable(string table)
		{
			connection.Open();
			SqlCeCommand command = new System.Data.SqlServerCe.SqlCeCommand("SELECT * FROM [" + table + "]", connection);
			SqlCeDataReader reader = command.ExecuteReader();
			string className = getClassName(table);
			List<object> arr = new List<object>();
			while (reader.Read())
			{
				object[] param = new object[reader.FieldCount];
				for (int i = 0; i < reader.FieldCount; i++) 
				{
					param[i] = reader.GetValue(i);
				}
				object item = Activator.CreateInstance(Type.GetType(className), param);
				arr.Add(item);
			}
			
			for (int j = 0; j < arr.Count; j++)		
			{
				control.Rows.Add(new DataGridViewRow());
				for (int i = 0; i < arr[0].GetType().GetProperties().Length; i++)
				{
					control.Rows[control.RowCount - 2].Cells[i].Value = arr[j].GetType().GetProperties().ToArray()[i].GetValue(arr[j], null);
				}
			}
			connection.Close();
			return control;
		}

		private void connectionClose()
		{
			if (connection.State == System.Data.ConnectionState.Open)
			{
				connection.Close();
			}
		}
		
		private void connectionOpen()
		{
			if (connection.State == System.Data.ConnectionState.Closed)
			{
				connection.Open();
			}
		}

		public void DeleteItemFormTable(string table, int id) 
		{
			connection.Open();
			SqlCeCommand command = new SqlCeCommand("DELETE [" + table + "] WHERE id='" + id.ToString() + "'", connection);
			command.ExecuteNonQuery();
		}

		private string className(string table)
		{
			string classN = "Kartwheel." + table.Substring(0, 1).ToUpper() + table.Substring(1, table.Length - 2);
			return classN;
		}
	}
}