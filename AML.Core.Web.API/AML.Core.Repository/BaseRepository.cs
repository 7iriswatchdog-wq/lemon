using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.RepositoryContract;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Dapper.FluentColumnMapping;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using Microsoft.Data.SqlClient;
using AML.Core.Common.StaticResource;
using System.Reflection.Metadata;
using System.Xml.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Document = iTextSharp.text.Document;

namespace AML.Core.Repository
{
    public class BaseRepository : IBaseRepository
    {
        public IConfiguration Configuration;
        private readonly IHttpContextAccessor _context;
        // Sql connection string for OHS database
        private string _ohsConnectionString = string.Empty;

        // column mapping collection
        private ColumnMappingCollection _mappings = null;
        IBaseRepository _baseRepository;
        public BaseRepository(IConfiguration configuration, IHttpContextAccessor context)
        {
            _mappings = new ColumnMappingCollection();
            _context = context;
            Configuration = configuration;

            _ohsConnectionString = configuration
                                .GetSection("Data:Ohs")
                                .GetSection("DbConnectionString").Value;
        }

        public BaseRepository(IBaseRepository baseRepository, IConfiguration configuration)
        {
            _baseRepository = baseRepository;
            Configuration = configuration;
        }

        public MySqlConnection GetConnections()
        {
            return new MySqlConnection(_ohsConnectionString);
        }

        /// <summary>
        /// Gets HealthCheck for Base Repository. Declared virtual for other Repositories to override.
        /// </summary>
        /// <returns>UTC datetime</returns>
        public virtual async Task<object> HealthCheckAsync()
        {
            DateTime currentUtcDateTime = DateTime.MinValue;

            var sql = "Select GetUtcDate()";

            currentUtcDateTime = await GetFirstOrDefaultAsync<DateTime>(sql, null, commandType: CommandType.Text);

            return currentUtcDateTime;

        }

        /// <summary>
        /// Returns the first result set based on the type of the requested entity.
        /// Example : To get details of an address based on address id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public T GetFirstOrDefault<T>(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            T result = default(T);
            Dapper.SqlMapper.SetTypeMap(
                       typeof(T),
                       new CustomPropertyTypeMap(
                        typeof(T),
                      (type, columnName) =>
                          type.GetProperties().FirstOrDefault(prop =>
              prop.GetCustomAttributes(false)
                  .OfType<ColumnAttribute>()
                  .Any(attr => attr.Name == columnName))));
            using (IDbConnection conn = GetConnections())
            {
                result = conn.QueryFirstOrDefault<T>(sql, parameters, null, commandTimeout, commandType);
            }

            return result;
        }

        /// <summary>
        /// Returns the first result set based on the type of the requested entity.
        /// Example : To get details of an address based on address id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public async Task<T> GetFirstOrDefaultAsync<T>(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            T result = default(T);

            using (IDbConnection conn = GetConnections())
            {
                result = await conn.QueryFirstOrDefaultAsync<T>(sql, parameters, null, commandTimeout, commandType);
            }

            return result;
        }

        /// <summary>
        /// Returns an collection of the type of requested entity.
        /// Example: To list of all address.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable<T> result = default(IEnumerable<T>);
            Dapper.SqlMapper.SetTypeMap(
                        typeof(T),
                        new CustomPropertyTypeMap(
                         typeof(T),
                       (type, columnName) =>
                           type.GetProperties().FirstOrDefault(prop =>
               prop.GetCustomAttributes(false)
                   .OfType<ColumnAttribute>()
                   .Any(attr => attr.Name == columnName))));
            using (IDbConnection conn = GetConnections())
            {
                result = conn.Query<T>(sql, parameters, null, true, commandTimeout, commandType);
            }

            return result;
        }

        /// <summary>
        /// Returns an collection of the type of requested entity.
        /// Example: To list of all address.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param> 
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAsync<T>(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable<T> result = default(IEnumerable<T>);
            Dapper.SqlMapper.SetTypeMap(
                         typeof(T),
                         new CustomPropertyTypeMap(
                          typeof(T),
                        (type, columnName) =>
                            type.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .Any(attr => attr.Name == columnName))));
            using (IDbConnection conn = GetConnections())
            {
                result = await conn.QueryAsync<T>(sql, parameters, null, commandTimeout, commandType);
            }

            return result;
        }

        /// <summary>
        /// Returns an of the type of requested entity.
        /// Example: To list of all address.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param> 
        /// <returns></returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = default(T);
            Dapper.SqlMapper.SetTypeMap(
                         typeof(T),
                         new CustomPropertyTypeMap(
                          typeof(T),
                        (type, columnName) =>
                            type.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .Any(attr => attr.Name == columnName))));
            using (IDbConnection conn = GetConnections())
            {
                result = await conn.QueryFirstOrDefaultAsync<T>(sql, parameters, null, commandTimeout, commandType);
            }

            return result;
        }


        /// <summary>
        /// Adds an entity. 
        /// The onus of getting the Id of the newly added entity is on each class that implements this API.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param> 
        /// <returns></returns>
        public int Add(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = 0;

            using (IDbConnection conn = GetConnections())
            {
                result = conn.Execute(sql, parameters, null, commandTimeout, commandType);
            }

            return result;
        }

        /// <summary>
        /// Adds an entity. 
        /// The onus of getting the Id of the newly added entity is on each class that implements this API.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param> 
        /// <returns></returns>
        public async Task<int> AddAsync(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = 0;

            using (IDbConnection conn = GetConnections())
            {
                result = await conn.ExecuteAsync(sql, parameters, null, commandTimeout, commandType);
            }

            return result;
        }

        public async Task<long> InsertAsync(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (IDbConnection conn = GetConnections())
            {
                var result = await conn.ExecuteScalarAsync<long>(sql, parameters, null, commandTimeout, commandType);
                return result;
            }
        }

        /// <summary>
        /// Updates an entity. 
        /// The onus of getting the Id of the newly added entity is on each class that implements this API.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param>  
        /// <returns></returns>
        public async Task<int> UpdateAsync(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.AddAsync(sql, parameters, commandTimeout, commandType);
        }

        /// <summary>
        /// Deletes an entity. 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param>   
        protected async Task DeleteAsync(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (IDbConnection conn = GetConnections())
            {
                await conn.ExecuteAsync(sql, parameters, null, commandTimeout, commandType);
            }
        }

        /// <summary>
        /// Returns multiple results based on the types specified.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="databaseName"></param>   
        /// <returns></returns>
        public async Task<dynamic> GetMultipleAsync<T1, T2>(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            dynamic result = new ExpandoObject();

            using (IDbConnection conn = GetConnections())
            {
                using (var multi = await conn.QueryMultipleAsync(sql, parameters, null, commandTimeout, commandType))
                {
                    var t1 = multi.Read<T1>();
                    var t2 = multi.Read<T2>();

                    result.Item1 = t1;
                    result.Item2 = t2;
                }
            }

            return result;
        }

        /// <summary>
        /// Registers the list of properties which you would like to map to it's corresponsing sql column name.
        /// This is usually used where your object's property name is different from the underline sql column name.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="columns">list fo columns</param>
        public void RegisterColumnMappings<T>(IEnumerable<SqlColumnMapping> columns)
        {
            // register only if the type is not already added to the mapping collection
            if (!_mappings.Mappings.Keys.Contains(typeof(T)))
            {
                var mappedType = _mappings.RegisterType<T>();

                foreach (var item in columns)
                {
                    //mappedType.MapProperty(item.Source).ToColumn(item.Target);
                }

                // register with dapper
                _mappings.RegisterWithDapper();
            }
        }



        /// <summary>
        /// Returns the sql connection based on the database against which the comnection is requested for.
        /// The default is against  database.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        private IDbConnection GetConnection()
        {
            string connectionString = _ohsConnectionString;
            connectionString = _ohsConnectionString;
            return new SqlConnection(connectionString);
        }
        /// <summary>
        /// Exceutes the sql command and returns the  first result 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (IDbConnection conn = GetConnections())
            {
                return conn.ExecuteScalar(sql, parameters, null, commandTimeout, commandType);
            }
        }

        public int Execute(string sql, object parameters = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (IDbConnection conn = GetConnections())
            {
                return conn.Execute(sql, parameters, null, commandTimeout, commandType);
            }
        }

        public class TextEvents : PdfPageEventHelper
        {
            PdfContentByte cb;
            PdfTemplate footerTemplate;
            BaseFont bf = null;
            DateTime PrintTime = DateTime.Now;
            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                try
                {
                    PrintTime = DateTime.Now;
                    bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    cb = writer.DirectContent;
                    footerTemplate = cb.CreateTemplate(50, 50);
                }
                catch (DocumentException de)
                {
                }
                catch (System.IO.IOException ioe)
                {
                }
            }

            public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
            {
                base.OnEndPage(writer, document);
                iTextSharp.text.Font baseFontNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
                iTextSharp.text.Font baseFontBig = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
                PdfPTable pdfTab = new PdfPTable(3);

                PdfPCell pdfCell1 = new PdfPCell();
                PdfPCell pdfCell3 = new PdfPCell();
                String text = "Page " + writer.CurrentPageNumber;

                {
                    cb.BeginText();
                    cb.SetFontAndSize(bf, 12);
                    cb.SetTextMatrix(document.PageSize.GetRight(320), document.PageSize.GetBottom(30));
                    cb.ShowText(text);
                    cb.EndText();
                }

                pdfCell1.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell3.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                pdfCell1.Border = 0;
                pdfCell3.Border = 0;
                pdfTab.AddCell(pdfCell1);
                pdfTab.AddCell(pdfCell3);


                pdfTab.TotalWidth = document.PageSize.Width - 80f;
                pdfTab.WidthPercentage = 70;

                pdfTab.WriteSelectedRows(0, -1, 40, document.PageSize.Height - 30, writer.DirectContent);

                cb.MoveTo(40, document.PageSize.GetBottom(50));
                cb.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(50));
                cb.Stroke();

            }

            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                base.OnCloseDocument(writer, document);

                footerTemplate.BeginText();
                footerTemplate.SetFontAndSize(bf, 12);
                footerTemplate.SetTextMatrix(0, 0);
                footerTemplate.ShowText((writer.PageNumber - 1).ToString());
                footerTemplate.EndText();
            }
        }
    }
}
