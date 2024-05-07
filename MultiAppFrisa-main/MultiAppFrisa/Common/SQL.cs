using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;
using MultiAppFrisa.Models;
using System.Data.SqlTypes;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Numerics;

namespace MultiAppFrisa.Common
{
    public class SQL
    {
        private static ThreadPoolTimer SQLTimer;
        private static SqlConnection connection;
        private static string connectionString = "Data Source=DESKTOP-RKOCNKA;Initial Catalog=Lepton; User Id=nuvahub; Password=asd;Connect Timeout = 10000";
        // private static string connectionString = "Data Source=DESKTOP-N277R4S\\SQLEXPRESS;Initial Catalog=Lepton; User Id=nh; Password=123;Connect Timeout = 10000";

        public class TemperatureData
        {
            
            public int Id { get; set; }

            [BsonElement("Temperatures")]
            public BsonArray Temperatures { get; set; }


            public DateTime Hora { get; set; }
            public byte[] ImageBytes { get; set; }
        }
        public static void RunProcess()
        {
            try
            {
                SQLTimer ??= ThreadPoolTimer.CreateTimer(SQLProcess, TimeSpan.FromMilliseconds(1000));
            }
            catch
            {

            }
        }
        
        private static async void SQLProcess(ThreadPoolTimer timer)
        {
            try
            {
                
                while (true)
                {
                    
                    if (Lepton.lastDataUpdate.AddMilliseconds(Lepton.milisecondsToUpdateData) <= DateTime.Now)
                    {
                        Lepton.lastDataUpdate = DateTime.Now;                        
                        var client = new MongoClient("mongodb://localhost:27017");//Conectamos a la base mongo
                        var database = client.GetDatabase("FlirCameraTest");
                        IMongoCollection<TemperatureData> collectionTemperature = database.GetCollection<TemperatureData>("TemperatureValueA400-1");
                        IMongoCollection<TemperatureData> collectionTemperature2 = database.GetCollection<TemperatureData>("TemperatureValueA400-1");

                        if (collectionTemperature != null)
                        {
                            var sort = Builders<TemperatureData>.Sort.Descending("_id");//ara extraer el valor de la ultima id de la base
                            var options = new FindOptions<TemperatureData> { Sort = sort, Limit = 1 };
                            var cursor = await collectionTemperature.FindAsync(FilterDefinition<TemperatureData>.Empty, options);
                            var lastDocument = await cursor.FirstOrDefaultAsync();
                            var currentId = lastDocument.Id;

                            var filter2 = Builders<TemperatureData>.Filter.Eq("_id", currentId); // Filtrar por el campo 'id'
                            var cursor2 = await collectionTemperature2.FindAsync(filter2);
                            

                            while (await cursor2.MoveNextAsync())
                            {
                                var batch = cursor2.Current;
                                
                                foreach (var document in batch)
                                {

                                    var temperaturesArray = document.Temperatures.ToArray(); //obtenemos el valor del array pero tenedremos que pasarlo a una lista ya que es lo que acepta la clase Lepton
                                    var temperaturesList = new List<float>();
                                    DateTime horaLocalMexico = ConvertirHoraUtcAHoraLocalMexico(document.Hora);
                                    Lepton.LastTemperatureDataUpdate = horaLocalMexico;
                                    foreach (var temperaturesArray2 in temperaturesArray.AsEnumerable())
                                    {
                                        foreach (var temperature in temperaturesArray2.AsBsonArray)
                                        {
                                            try
                                            {
                                                // Convertir el valor a float y agregarlo a la lista
                                                var temperatureFloat = Convert.ToSingle(temperature);
                                                temperaturesList.Add(temperatureFloat);
                                            }
                                            catch (FormatException ex)
                                            {

                                            }
                                        }
                                    }
                                    Lepton.temperatures = temperaturesList; //metemos los datos de la temperatura
                                    Lepton.isNewData = true;
                                    
                                    
                                    SqlBinary sqlBinary = document.ImageBytes; //obtenemos la imagen
                                    Lepton.originalImage = await SqlBinaryToSoftwareBitmapAsync(sqlBinary); //la convertimos a la clase que necesitamos

                                    Lepton.coloredImage = Lepton.originalImage;

                                }
                            }
                            
                        }
                        // string query = "SELECT * FROM Lepton_Image WHERE id = 1";
                        //SqlDataReader reader = await Read(query);                        
                        //  if (reader != null)
                        // {
                        //    while (reader.Read())
                        //   {
                         //  SqlBinary binary = reader.GetSqlBinary(2);
                        //     string json = reader.GetString(3);

                       // Lepton.originalImage = await SqlBinaryToSoftwareBitmapAsync(binary);
                          //      Lepton.temperatures = JsonConvert.DeserializeObject<List<float>>(json);
                          //      Lepton.isNewData = true;
                          //  }

                          //  reader.Close();
                       // }
                    }

                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                await Task.Delay(1000);
                SQLTimer = ThreadPoolTimer.CreateTimer(SQLProcess, TimeSpan.FromMilliseconds(100));
            }
        }

        private static DateTime ConvertirHoraUtcAHoraLocalMexico(DateTime horaUtc)
        {
            // Establecer la zona horaria de México (Central Standard Time (Mexico))
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

            // Convertir la hora UTC a la hora local de México
            DateTime horaLocalMexico = TimeZoneInfo.ConvertTimeFromUtc(horaUtc, cstZone);

            return horaLocalMexico;
        }

        public static async Task<SqlDataReader> Read(string query)
        {
            SqlDataReader response = null;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(connectionString);
                }

                if (connection.State != ConnectionState.Open)
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(10000));
                    await connection.OpenAsync(cancellationTokenSource.Token);
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

                    response = await command.ExecuteReaderAsync(cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {

            }

            return response;
        }

        public static async Task<int> Write(string query)
        {
            int rowsAffected = 0;

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(connectionString);
                }

                if (connection.State != ConnectionState.Open)
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));
                    await connection.OpenAsync(cancellationTokenSource.Token);
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

                    rowsAffected = await command.ExecuteNonQueryAsync(cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {

            }

            return rowsAffected;
        }

        private static async Task<SoftwareBitmap> SqlBinaryToSoftwareBitmapAsync(SqlBinary binary)
        {
            SoftwareBitmap result = null;
            try
            {
                byte[] bytes = binary.Value;
                //String data = Encoding.UTF8.GetString(bytes);
                //byte[] decoded = Convert.FromBase64String(data);
                var imageMemoryStream = new InMemoryRandomAccessStream();
                await imageMemoryStream.WriteAsync(bytes.AsBuffer());
                imageMemoryStream.Seek(0);
                BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(imageMemoryStream);
                result = await bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
               
           

            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}