using DevExpress.CodeParser;
using DevExpress.XtraEditors;
using MySqlConnector;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace vsHelp.Classes
{
    public class Conexao
    {
        private static IniFile config_ini = new(@"C:\Visual Software\MyCommerce\Config.ini"); // Manter, caso seja usado para outras configurações no futuro
        private static string Ip = Properties.Conexao.Default.IPSERVIDOR;
        private static string Porta = Properties.Conexao.Default.PORTASERVIDOR;
        private static string Usuario = Properties.Conexao.Default.USUARIOSERVIDOR;
        private static string Senha = Properties.Conexao.Default.Senha;
        private static string BancoDeDados = Properties.Conexao.Default.DATABASE;
        public static MySqlConnection connection;

        private static Conexao instancia = new();
        public static Conexao Instancia => instancia;

        private Conexao()
        {
            connection = new($"Server={Ip};Port={Porta};User Id={Usuario};Password={Senha};Database={BancoDeDados};Allow User Variables=true;");

            try
            {
                connection.Open();
            }
            catch (MySqlException e)
            {
                if (e.ErrorCode == MySqlErrorCode.UnknownDatabase)
                {
                    connection = new($"Server={Ip};Port={Porta};User Id={Usuario};Password={Senha};");
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = $"create database `{BancoDeDados}`";
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    instancia = new();
                }
                else
                {
                    XtraMessageBox.Show(e.Message, "..::vsHelp::..");
                }
            }
        }

        public static void AtualizaConexao()
        {
            // Não precisa recarregar config_ini aqui, pois não o usamos para credenciais de conexão
            // config_ini = new(@"C:\Visual Software\MyCommerce\Config.ini"); 

            Ip = Properties.Conexao.Default.IPSERVIDOR;
            Porta = Properties.Conexao.Default.PORTASERVIDOR;
            Usuario = Properties.Conexao.Default.USUARIOSERVIDOR;
            Senha = Properties.Conexao.Default.Senha;
            BancoDeDados = Properties.Conexao.Default.DATABASE;

            instancia = new();
        }

        public static void AtualizaConexao(string ip, string porta, string usuario, string senha, string bancoDeDados)
        {
            Ip = ip;
            Porta = porta;
            Usuario = usuario;
            Senha = senha;
            BancoDeDados = bancoDeDados;

            Properties.Conexao.Default.Senha = senha;
            Properties.Conexao.Default.Save();

            instancia = new();
        }

        // Adicione estes dois métodos à sua classe Conexao
        public static bool BancoExiste(string servidor, string porta, string usuario, string senha, string nomeBanco)
        {
            try
            {
                string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};";
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{nomeBanco}'", conn);
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool CriarBanco(string servidor, string porta, string usuario, string senha, string nomeBanco)
        {
            try
            {
                string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};";
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand($"CREATE DATABASE `{nomeBanco}`", conn);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static List<string> ListarBancosDeDados(string servidor, string porta, string usuario, string senha)
        {
            List<string> databases = new List<string>();
            try
            {
                string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};";
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SHOW DATABASES;", conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes.
                System.Diagnostics.Debug.WriteLine($"Erro ao listar bancos de dados: {ex.Message}");
                // Opcionalmente, exibir uma mensagem para o usuário se este método for chamado diretamente da lógica da UI.
                // XtraMessageBox.Show($"Erro ao listar bancos de dados: {ex.Message}", "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return databases;
        }

        public static void RestaurarBanco(string servidor, string porta, string usuario, string senha, string nomeBanco, string caminho, ProgressBarControl pb)
        {
            string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};Database={nomeBanco};Allow User Variables=true;";
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();

                pb.Invoke(() =>
                {
                    pb.EditValue = 0;
                    pb.Properties.Maximum = Utils.GetQtdComandos(caminho);
                    pb.Properties.Step = 1;
                });

                using (StreamReader reader = new StreamReader(caminho))
                {
                    string linha;
                    StringBuilder sqlCommand = new StringBuilder();

                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (linha.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("USE", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("DELIMITER", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        sqlCommand.AppendLine(linha);

                        if (linha.Trim().EndsWith(";"))
                        {
                            using (MySqlCommand cmd = new MySqlCommand(sqlCommand.ToString(), conn))
                            {
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception)
                                {
                                    //
                                }
                                finally
                                {
                                    pb.Invoke(() =>
                                    {
                                        pb.PerformStep();
                                        pb.Update();
                                    });
                                }
                            }
                            sqlCommand.Clear();
                        }
                    }

                    if (sqlCommand.Length > 0)
                    {
                        using (MySqlCommand cmd = new MySqlCommand(sqlCommand.ToString(), conn))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception)
                            {
                                //
                            }
                            finally
                            {
                                pb.Invoke(() =>
                                {
                                    pb.PerformStep();
                                    pb.Update();
                                });
                            }
                        }
                    }
                }
            }

            Utils.Notificacao("Restauração", "Backup Restaurado");
        }
    }
}
