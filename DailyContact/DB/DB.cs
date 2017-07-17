using Android.OS;
using SQLite;

namespace DailyContact.DB
{
    public class DBFunctions
    {
        public string createDatabase(string path)
        {
            try
            {
                var connection = new SQLiteAsyncConnection(path);
                {
                    connection.CreateTableAsync<Message>();
                    return "Database created";
                }
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public async System.Threading.Tasks.Task<string> insertUpdateDataAsync(Message data, string path)
        {
            try
            {
                var db = new SQLiteAsyncConnection(path);
                if (await db.InsertAsync(data) != 0)
                    await db.UpdateAsync(data);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public async System.Threading.Tasks.Task<int> findNumberRecordsAsync(string path)
        {
            try
            {
                var db = new SQLiteAsyncConnection(path);
                // this counts all records in the database, it can be slow depending on the size of the database
                var count = await db.ExecuteScalarAsync<int>("Select count(*) from Message");

                // for a non-parameterless query
                // var count = db.ExecuteScalar<int>("SELECT Count(*) FROM Person WHERE FirstName="Amy");

                return count;
            }
            catch (SQLiteException ex)
            {
                return -1;
            }
        }
    }


}