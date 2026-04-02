using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using LMS_DL;
using System.Configuration;
using NLog;

namespace NPA_APPLICATION
{
    class Program
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
       public static void Main(string[] args)
       {
            logger.Info($"NPA process start !!");
            RunMainCode();
            logger.Info($"NPA process start !!");
       }

        static void RunMainCode()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["CrediCash_Dev"].ConnectionString;
                if (!string.IsNullOrEmpty(connectionString))
                {
                    List<NPAModel> nPAModels = GetIncompleteData(connectionString);
                    if (nPAModels.Count > 0)
                    {
                        foreach (var nPA in nPAModels)
                        {
                            logger.Info($"NPA process start: {nPA.loan_id}");
                            Nocupdate(connectionString, nPA);
                            logger.Info($"NPA process end: {nPA.loan_id}");
                        }
                    }
                }
                else
                {
                    logger.Error($"Information: NO any data found !!");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error while running main code logic: {ex.Message}");
            }
        }
        public static void Nocupdate(string connectionString , NPAModel nPA)
        {
            DataSet Objds = null;
            DataTable Objtable = new DataTable();
            SqlParameter[] param = new SqlParameter[2];

            param[0] = new SqlParameter("lead_id", SqlDbType.VarChar, 10);
            param[0].Value = nPA.lead_id;

            param[1] = new SqlParameter("loan_id", SqlDbType.VarChar, 30);
            param[1].Value = nPA.loan_id;

            using (var connection = new SqlConnection(connectionString))
            {
                Objds = new DataSet();
                try
                {
                    Objds = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "USP_set_NPA_loanid", param);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error fetching NPA UPdate Method NOC Update - : {ex.Message}");
                }
            }
        }
        public static List<NPAModel> GetIncompleteData(string dbconnection)
        {
            List<NPAModel> nPAModels = new List<NPAModel>();
            DataSet objDs = null;

            try
            {
                using (var connection = new SqlConnection(dbconnection))
                {
                    SqlParameter[] param = new SqlParameter[1];
                    objDs = SqlHelper.ExecuteDataset(dbconnection, CommandType.StoredProcedure, "USP_GetDPDloanid", param);
                }
                {
                    foreach (DataRow row in objDs.Tables[0].Rows)
                    {
                        NPAModel nPA = new NPAModel
                        {
                            loan_id = Convert.ToString(row["loan_id"]),
                            lead_id = Convert.ToString(row["lead_id"]),
                        };
                        nPAModels.Add(nPA);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error fetching incomplete data: {ex.Message}");
            }
            return nPAModels;
        }
    }
}
