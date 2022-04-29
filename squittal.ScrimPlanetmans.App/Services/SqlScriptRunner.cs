using System;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace squittal.ScrimPlanetmans.Services
{
    public class SqlScriptRunner : ISqlScriptRunner
    {
        private readonly string _sqlDirectory = "Data/SQL";
        private readonly string _basePath;
        private readonly string _scriptDirectory;
        private readonly string _adhocScriptDirectory;

        private readonly Server _server; // = new Server("(LocalDB)\\MSSQLLocalDB");

        private readonly string _ConnectionString;

        private readonly ILogger<SqlScriptRunner> _logger;

        public SqlScriptRunner(ILogger<SqlScriptRunner> logger, IConfiguration config)
        {
            _logger = logger;

            _ConnectionString = config.GetConnectionString("PlanetmansDbContext");

            _basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            _scriptDirectory = Path.Combine(_basePath, _sqlDirectory);

            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(config.GetConnectionString("PlanetmansDbContext"));
            //_server = new Server(builder.DataSource);

            _adhocScriptDirectory = Path.GetFullPath(Path.Combine(_basePath, "..", "..", "..", "../sql_adhoc"));
        }

        private void RunCommand(string text) {

            IEnumerable<string> commandStrings = Regex.Split(text, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            using (SqlConnection connection = new SqlConnection(_ConnectionString)) {
                connection.Open();
                
                foreach (string commandString in commandStrings) {
                    if (commandString.Trim() != "") {
                        using (var command = new SqlCommand(commandString, connection)) {
                            command.ExecuteNonQuery();
                            /*
                            try {
                            } catch (SqlException ex) { 
                                string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                //MessageBox.Show(string.Format("Please check the SqlServer script.\nFile: {0} \nLine: {1} \nError: {2} \nSQL Command: \n{3}", pathStoreProceduresFile, ex.LineNumber, ex.Message, spError), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            */
                        }
                    }
                }
            }
        }

        public void RunSqlScript(string fileName, bool minimalLogging = false)
        {
            var scriptPath = Path.Combine(_scriptDirectory, fileName);
            
            try
            {
                var scriptFileInfo = new FileInfo(scriptPath);

                string scriptText = scriptFileInfo.OpenText().ReadToEnd();

                //_server.ConnectionContext.ExecuteNonQuery(scriptText);

                RunCommand(scriptText);

                if (!minimalLogging)
                {
                    _logger.LogInformation($"Successfully ran sql script at {scriptPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running sql script {scriptPath}: {ex}");
            }
        }

        public bool TryRunAdHocSqlScript(string fileName, out string info, bool minimalLogging = false)
        {
            var scriptPath = Path.Combine(_adhocScriptDirectory, fileName);

            try
            {
                var scriptFileInfo = new FileInfo(scriptPath);

                string scriptText = scriptFileInfo.OpenText().ReadToEnd();

                RunCommand(scriptText);
                //_server.ConnectionContext.ExecuteNonQuery(scriptText);

                info = $"Successfully ran sql script at {scriptPath}";

                if (!minimalLogging) {
                    _logger.LogInformation(info);
                }

                return true;
            }
            catch (Exception ex)
            {
                info = $"Error running sql script {scriptPath}: {ex}";

                _logger.LogError(info);

                return false;
            }
        }

        public void RunSqlDirectoryScripts(string directoryName)
        {
            var directoryPath = Path.Combine(_scriptDirectory, directoryName);

            try
            {
                var files = Directory.GetFiles(directoryPath, "*.sql");

                foreach (var file in files) {
                    RunSqlScript(file, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running SQL scripts in directory {directoryName}: {ex}");
            }
        }
    }
}
