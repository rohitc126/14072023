using BusinessLayer.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DAL
{
    public class DAL_MATERIAL_REQUISITION_ENTRY
    {
        public string INSERT_MATERIAL_REQUISITION_ENTRY(string Emp_Code, Material_Requisition_Entry MAT, out ResultMessage oblMsg)
        {
            string errorMsg = "";
            oblMsg = new ResultMessage();
         
            using (var connection = new SqlConnection(sqlConnection.GetConnectionString_SGX()))
            {
                connection.Open();
                SqlCommand command;
                SqlTransaction transactionScope = null;
                transactionScope = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {

                    SqlParameter[] param =
                                    {
                                       new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200) 
                                       ,new SqlParameter("@REQUISITION_ID", SqlDbType.Decimal) 
                                      ,new SqlParameter("@REQUISITION_CODE", SqlDbType.VarChar, 20)
                                       
                                      ,new SqlParameter("@REQUISITION_DATE", MAT.Requisition_DT)
                                      ,new SqlParameter("@BRANCH_CODE",MAT.BRANCH_CODE)  
                                      ,new SqlParameter("@MATERIAL_ID",MAT.Material_Id)             
                                      ,new SqlParameter("@QUANTITY",  MAT.Qty) 
                                      ,new SqlParameter("@UNIT", MAT.Unit)
                                      ,new SqlParameter("@EXPECTED_DEL_DATE",MAT.Expected_Del_DT)
                                      ,new SqlParameter("@ADDED_BY",Emp_Code)  
                    
                    };

                    param[0].Direction = ParameterDirection.Output;
                    param[1].Direction = ParameterDirection.Output;
                    param[2].Direction = ParameterDirection.Output;

                    new DataAccess().InsertWithTransaction("[AGG].[USP_INSERT_MATERIAL_REQUISITION_ENTRY]", CommandType.StoredProcedure, out command, connection, transactionScope, param);
                    decimal REQUISITION_ID = (decimal)command.Parameters["@REQUISITION_ID"].Value;
                    string REQUISITION_CODE = (string)command.Parameters["@REQUISITION_CODE"].Value;
                    string error_1 = (string)command.Parameters["@ERRORSTR"].Value;
                    if (REQUISITION_ID == -1) { errorMsg = error_1; }


                   
                        if (errorMsg == "")
                        {

                            oblMsg.ID = REQUISITION_ID;
                            oblMsg.CODE = REQUISITION_CODE;
                            oblMsg.MsgType = "Success";
                            transactionScope.Commit();

                        }
                    

                    else
                    {
                        oblMsg.Msg = errorMsg;
                        oblMsg.MsgType = "Error";
                        transactionScope.Rollback();
                    }
                    }
                
                catch (Exception ex)
                {

                    try
                    {
                        transactionScope.Rollback();
                    }
                    catch (Exception ex1)
                    {
                        errorMsg = "Error: Exception occured. " + ex1.StackTrace.ToString();
                    }

                }
                finally
                {
                    connection.Close();
                }
            }
            return errorMsg;
        }

        public List<MATERIAL_REQ_DATA_LIST> Select_Material_Data_List(Material_Req_List materiallist)
        {
            SqlParameter[] param = {  
                                      
                                       new SqlParameter("@BRANCH_CODE", materiallist.BRANCH_CODE),
                                       new SqlParameter("@FROM_DT", materiallist.From_DT),
                                       new SqlParameter("@TO_DT", materiallist.To_DT) 
                                   };

            DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_SELECT_MATERIAL_REQUISITION_LIST]", CommandType.StoredProcedure, param);

            List<MATERIAL_REQ_DATA_LIST> _list = new List<MATERIAL_REQ_DATA_LIST>();
            DataTable dt = ds.Tables[0];
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    _list.Add(new MATERIAL_REQ_DATA_LIST
                    {
                        REQUISITION_ID = Convert.ToDecimal(row["REQUISITION_ID"]),
                        REQUISITION_CODE = Convert.ToString(row["REQUISITION_CODE"]),
                        REQUISITION_DATE = Convert.ToString(row["REQUISITION_DATE"]),
                        Material_Name = Convert.ToString(row["Material_Name"]),
                        QUANTITY = Convert.ToDecimal(row["QUANTITY"]),
                        UNIT = Convert.ToString(row["UNIT"]),
                        EXPECTED_DEL_DATE = Convert.ToString(row["EXPECTED_DEL_DATE"]),
                        MREC_ID =Convert.ToDecimal(row["MREC_ID"] == DBNull.Value ? 0 : row["MREC_ID"]),
                        RECEIPT_STATUS = Convert.ToString(row["RECEIPT_STATUS"] == DBNull.Value ? "" : row["RECEIPT_STATUS"]),
                    });
                }
            }

            return _list;
        }
        public Material_Receipt_Entry Edit_RAW_MATERIAL_RECEIPT_ENTRY(decimal REQUISITION_ID)
        {
            SqlParameter[] param = { new SqlParameter("@REQUISITION_ID", REQUISITION_ID) };

            DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_EDIT_MATERIAL_REQUISITION_ENTRY]", CommandType.StoredProcedure, param);
            Material_Receipt_Entry _objRAWMAt = new Material_Receipt_Entry();

            List<MATERIAL_REQ_DATA_LIST> _list = new List<MATERIAL_REQ_DATA_LIST>();
            MATERIAL_REQ_DATA_LIST dtl = null;
            DataTable dt = ds.Tables[0];
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                 dtl = new MATERIAL_REQ_DATA_LIST();
                _objRAWMAt.REQUISITION_ID = Convert.ToInt32(dt.Rows[0]["REQUISITION_ID"]);
                _objRAWMAt.REQUISITION_CODE = Convert.ToString(dt.Rows[0]["REQUISITION_CODE"]);
                _objRAWMAt.QUANTITY = Convert.ToDecimal(dt.Rows[0]["QUANTITY"]);
                _objRAWMAt.UNIT = Convert.ToString(dt.Rows[0]["UNIT"]);
              

            }
           
            return _objRAWMAt;
        }

        public List<VendorName_Master> GetVendorList(string loc, string emp)
        {
            SqlParameter[] param = { new SqlParameter("@loc", loc), new SqlParameter("@emp", emp) };
            DataTable dt = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataTable("[dbo].[usp_FillVendor]", CommandType.StoredProcedure, param);
            List<VendorName_Master> list = new List<VendorName_Master>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new VendorName_Master
                    {
                        VendorCode = Convert.ToString(row["VendorCode"] == DBNull.Value ? "" : row["VendorCode"]),
                        VendorName = Convert.ToString(row["VendorName"] == DBNull.Value ? "" : row["VendorName"]),

                    });
                }
            }
            return list;
        }



        public string INSERT_MATERIAL_RECEIPT_ENTRY(string Emp_Code, Material_Receipt_Entry _RECEIPT, out ResultMessage oblMsg)
        {
            
            string errorMsg = "";
            oblMsg = new ResultMessage();
            using (var connection = new SqlConnection(sqlConnection.GetConnectionString_SGX()))
            {
                connection.Open();
                SqlCommand command;
                SqlTransaction transactionScope = null;
                transactionScope = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    int IS_FILE_UPLOAD = 0;

                    string DMS_Path = ConfigurationManager.AppSettings["DMSPATH"].ToString();
                    string filePath = "REPORT\\MATERIAL RECEIPT ENTRY\\";
                    string directoryPath = DMS_Path + filePath;
                    string ext = "";
                    if (_RECEIPT.UploadFile != null)
                    {
                        IS_FILE_UPLOAD = 1;
                        ext = new System.IO.FileInfo(_RECEIPT.UploadFile.FileName).Extension;

                    }
                    SqlParameter[] param =
                    { 
                      new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200)
                     ,new SqlParameter("@MREC_ID", SqlDbType.Decimal)
                     ,new SqlParameter("@MREC_CODE", SqlDbType.VarChar, 20)  
                     ,new SqlParameter("@REQUISITION_ID",(_RECEIPT.REQUISITION_ID == null)?(object)DBNull.Value : _RECEIPT.REQUISITION_ID) 

                     ,new SqlParameter("@QUANTITY",(_RECEIPT.QUANTITY == null)?(object)DBNull.Value : _RECEIPT.QUANTITY) 
                     ,new SqlParameter("@RATE",  (_RECEIPT.RATE == null)?(object)DBNull.Value : _RECEIPT.RATE)
                     ,new SqlParameter("@UNIT",  _RECEIPT.UNIT)
                     ,new SqlParameter("@AMOUNT", (_RECEIPT.AMOUNT == null)?(object)DBNull.Value : _RECEIPT.AMOUNT)
                     ,new SqlParameter("@TAX_PER",  (_RECEIPT.Taxes == null)?(object)DBNull.Value : _RECEIPT.Taxes)
                     ,new SqlParameter("@TAX_AMT", (_RECEIPT.txtAmount == null)?(object)DBNull.Value : _RECEIPT.txtAmount)
                     ,new SqlParameter("@TOTAL_AMT", (_RECEIPT.TOTAL_AMT == null)?(object)DBNull.Value : _RECEIPT.TOTAL_AMT)
                     ,new SqlParameter("@PO_NO",  _RECEIPT.PO_NO)
                     ,new SqlParameter("@VENDOR_CODE", _RECEIPT.VENDOR_CODE)
                     ,new SqlParameter("@TRANSPORT_CHARGES", (_RECEIPT.Transport_Charges == null)?(object)DBNull.Value : _RECEIPT.Transport_Charges)
                     ,new SqlParameter("@MAMOOL_CHARGES", (_RECEIPT.Mamool_Charges == null)?(object)DBNull.Value : _RECEIPT.Mamool_Charges)
                     ,new SqlParameter("@OTHER_CHARGES", (_RECEIPT.Other_Charges == null)?(object)DBNull.Value : _RECEIPT.Other_Charges)
                     ,new SqlParameter("@TOTAL_EXPENSE", (_RECEIPT.Total_Expense == null)?(object)DBNull.Value : _RECEIPT.Total_Expense)
                     ,new SqlParameter("@OVERALL_COST",(_RECEIPT.Overall_Cost == null)?(object)DBNull.Value : _RECEIPT.Overall_Cost)
                     ,new SqlParameter("@ADDED_BY", Emp_Code) 
                     ,new SqlParameter("@IS_FILE_UPLOAD", IS_FILE_UPLOAD)  
                     ,new SqlParameter("@FILE_PATH", string.IsNullOrEmpty(ext)?(object)DBNull.Value:ext) 
                    };

                    param[0].Direction = ParameterDirection.Output;
                    param[1].Direction = ParameterDirection.Output;
                    param[2].Direction = ParameterDirection.Output;

                    new DataAccess().InsertWithTransaction("[AGG].[USP_INSERT_MATERIAL_RECEIPT_ENTRY]", CommandType.StoredProcedure, out command, connection, transactionScope, param);
                    decimal MREC_ID = (decimal)command.Parameters["@MREC_ID"].Value;
                    string MREC_CODE = (string)command.Parameters["@MREC_CODE"].Value;
                    string error_1 = (string)command.Parameters["@ERRORSTR"].Value;

                    if (MREC_ID == -1) { errorMsg = error_1; }

                    if (errorMsg == "")
                    {
                        // Below code is used for attached slip file
                        if (_RECEIPT.UploadFile != null)
                        {
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            if (_RECEIPT.UploadFile != null)
                            {
                                string fileName = MREC_CODE.Replace("/", "_") + ext;
                                _RECEIPT.UploadFile.SaveAs(directoryPath + fileName);
                            }
                        }

                        oblMsg.ID = MREC_ID;
                        oblMsg.CODE = MREC_CODE;
                        oblMsg.MsgType = "Success";
                        transactionScope.Commit();
                    }
                    else
                    {
                        oblMsg.Msg = errorMsg;
                        oblMsg.MsgType = "Error";
                        transactionScope.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        transactionScope.Rollback();
                    }
                    catch (Exception ex1)
                    {
                        errorMsg = "Error: Exception occured. " + ex1.StackTrace.ToString();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return errorMsg;
        }

        public Material_Receipt_Entry VIEW_RAW_MATERIAL_RECEIPT_ENTRY(decimal REQUISITION_ID)
        {
             
            SqlParameter[] param = { new SqlParameter("@REQUISITION_ID", @REQUISITION_ID) };

            DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_VIEW_MATERIAL_RECEIPT_ENTRY]", CommandType.StoredProcedure, param);
            Material_Receipt_Entry _objReceipMAt = null;

                DataTable dt = ds.Tables[0];
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        _objReceipMAt = new Material_Receipt_Entry();
                        _objReceipMAt.REQUISITION_ID = Convert.ToInt32(row["MREC_ID"] == DBNull.Value ? 0 : row["MREC_ID"]);
                        _objReceipMAt.REQUISITION_CODE = Convert.ToString(row["MREC_CODE"]);
                        _objReceipMAt.QUANTITY = Convert.ToDecimal(row["QUANTITY"] == DBNull.Value ? 0 : row["QUANTITY"]);
                        _objReceipMAt.RATE = Convert.ToDecimal(row["RATE"] == DBNull.Value ? 0 : row["RATE"]);
                        _objReceipMAt.UNIT = Convert.ToString(row["UNIT"]);
                        _objReceipMAt.AMOUNT = Convert.ToDecimal(row["AMOUNT"] == DBNull.Value ? 0 : row["AMOUNT"]);
                        _objReceipMAt.Taxes = Convert.ToDecimal(row["TAX_PER"] == DBNull.Value ? 0 : row["TAX_PER"]);
                        _objReceipMAt.txtAmount = Convert.ToDecimal(row["TAX_AMT"] == DBNull.Value ? 0 : row["TAX_AMT"]);
                        _objReceipMAt.TOTAL_AMT = Convert.ToDecimal(row["TOTAL_AMT"] == DBNull.Value ? 0 : row["TOTAL_AMT"]);
                        _objReceipMAt.PO_NO = Convert.ToString(row["PO_NO"]);
                        _objReceipMAt.VENDOR_NAME = Convert.ToString(row["VendorName"]);
                        _objReceipMAt.Transport_Charges = Convert.ToDecimal(row["TRANSPORT_CHARGES"] == DBNull.Value ? 0 : row["TRANSPORT_CHARGES"]);
                        _objReceipMAt.Mamool_Charges = Convert.ToDecimal(row["MAMOOL_CHARGES"] == DBNull.Value ? 0 : row["MAMOOL_CHARGES"]);
                        _objReceipMAt.Other_Charges = Convert.ToDecimal(row["OTHER_CHARGES"] == DBNull.Value ? 0 : row["OTHER_CHARGES"]);
                        _objReceipMAt.Total_Expense = Convert.ToDecimal(row["TOTAL_EXPENSE"] == DBNull.Value ? 0 : row["TOTAL_EXPENSE"]);
                        _objReceipMAt.Overall_Cost = Convert.ToDecimal(row["OVERALL_COST"] == DBNull.Value ? 0 : row["OVERALL_COST"]);
                        _objReceipMAt.FILE_PATH = Convert.ToString(row["FILE_PATH"]);
                        _objReceipMAt.IS_FILE_UPLOAD = Convert.ToInt32(dt.Rows[0]["IS_FILE_UPLOAD"]);
                }

            }
         
            return _objReceipMAt;
        }







        //public List<Specific_Gravity_Abs> SELECT_SieveAnalysis_Data_List(Specific_Gravity_Abs _Sieve)
        //{
        //    SqlParameter[] param = { 
        //                               new SqlParameter("@Test_No", _Sieve.Test_No),
        //                               new SqlParameter("@Test_Date", _Sieve.Date ),
        //                               new SqlParameter("@Type", _Sieve.Type),
        //                               new SqlParameter("@Colour", _Sieve.Colour),
        //                               new SqlParameter("@Texture", _Sieve.Texture),
        //                               new SqlParameter("@Shape", _Sieve.Shape),
        //                                new SqlParameter("@Rock_Type", _Sieve.Rock_Type)
        //                           };

        //    DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_SELECT_SPECIFIC_GRAVITY]", CommandType.StoredProcedure, param);

        //    List<Specific_Gravity_Abs> _list = new List<Specific_Gravity_Abs>();
        //    DataTable dt = ds.Tables[0];
        //    if (ds != null && ds.Tables[0].Rows.Count > 0)
        //    {
        //        foreach (DataRow row in ds.Tables[0].Rows)
        //        {
        //            _list.Add(new Specific_Gravity_Abs
        //            {
        //                Test_ID = Convert.toin(row["SAH_ID"]),
        //                Test_No = Convert.ToString(row["SAH_CODE"]),
        //                Branch_Name = Convert.ToString(row["Branch_Name"]),
        //                SIZE = Convert.ToString(row["SIZE"]),
        //                SAMPLING_FROM = Convert.ToString(row["SAMPLING_FROM"]),
        //                PROPOSE = Convert.ToString(row["PROPOSE"]),
        //                SAMPLING_DATE = Convert.ToString(row["SAMPLING_DATE"]),
        //                TESTING_DATE = Convert.ToString(row["TESTING_DATE"]),
        //                REMARKS = Convert.ToString(row["REMARKS"]),

        //                TESTED_BY_CODE = Convert.ToString(row["TESTED_BY_CODE"]),
        //                TESTED_BY = Convert.ToString(row["TESTED_BY"]),
        //                CHECKED_BY_CODE = Convert.ToString(row["CHECKED_BY_CODE"]),
        //                CHECKED_BY = Convert.ToString(row["CHECKED_BY"]),
        //                ADDED_BY_CODE = Convert.ToString(row["ADDED_BY_CODE"]),
        //                ADDED_BY = Convert.ToString(row["ADDED_BY"]),
        //                IS_LOCKED = Convert.ToInt32(row["IS_LOCKED"])

        //            });
        //        }
        //    }

        //    return _list;
        //}



        public string INSERT_SPECIFIC_GRAVITY(string Emp_Code, Specific_Gravity_Abs _smixDesign, out ResultMessage oblMsg)
        {

            string errorMsg = "";
            oblMsg = new ResultMessage();
            using (var connection = new SqlConnection(sqlConnection.GetConnectionString_SGX()))
            {

                connection.Open();
                SqlCommand command;
                SqlTransaction transactionScope = null;
                transactionScope = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {

                    SqlParameter[] param =
                    { 
                      new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200)
                     ,new SqlParameter("@Test_ID", SqlDbType.Decimal) 
                     ,new SqlParameter("@Test_No", SqlDbType.VarChar, 20)  
                     ,new SqlParameter("@Test_Date", _smixDesign.Date)
                     ,new SqlParameter("@Source", _smixDesign.Source)  
                     ,new SqlParameter("@Type", _smixDesign.Type )    
                     ,new SqlParameter("@Colour", _smixDesign.Colour)   
                     ,new SqlParameter("@Texture", _smixDesign.Texture) 
                     ,new SqlParameter("@Shape", _smixDesign.Shape)  
                     ,new SqlParameter("@Rock_Type", _smixDesign.Rock_Type) 
                     ,new SqlParameter("@Added_By",Emp_Code)  
                     //,new SqlParameter("@Detemination", _smixDesign.DETRMINATION)
                    ,new SqlParameter("@Sample_Wt" , (_smixDesign.Sample_Wt == null)? (object)DBNull.Value : _smixDesign.Sample_Wt)                   
                  ,new SqlParameter("@Wb_Agg_Wt", (_smixDesign.Wb_Agg_Wt == null)? (object)DBNull.Value : _smixDesign.Wb_Agg_Wt)   
                   ,new SqlParameter("@Dry_Agg_Wt", (_smixDesign.Dry_Agg_Wt == null)? (object)DBNull.Value : _smixDesign.Dry_Agg_Wt) 
                                      ,new SqlParameter("@Oven_Dry_Agg_Wt", (_smixDesign.Oven_Dry_Agg_Wt == null)?(object)DBNull.Value : _smixDesign.Dry_Agg_Wt)   
                    };

                    param[0].Direction = ParameterDirection.Output;
                    param[1].Direction = ParameterDirection.Output;
                    param[2].Direction = ParameterDirection.Output;
                    new DataAccess().InsertWithTransaction("[AGG].[USP_INSERT_SPECIFIC_GRAVITY_HDR]", CommandType.StoredProcedure, out command, connection, transactionScope, param);
                    decimal Test_ID = (decimal)command.Parameters["@Test_ID"].Value;
                    string Test_No = (string)command.Parameters["@Test_No"].Value;
                    string error_1 = (string)command.Parameters["@ERRORSTR"].Value;

                    if (Test_ID == -1) { errorMsg = error_1; }

                        if (errorMsg == "")
                        {

                            oblMsg.ID = Test_ID;
                            oblMsg.CODE = Test_No;
                            oblMsg.MsgType = "Success";
                            transactionScope.Commit();
                        }
                        else
                        {
                            oblMsg.Msg = errorMsg;
                            oblMsg.MsgType = "Error";
                            transactionScope.Rollback();
                        }

                    }
                
                catch (Exception ex)
                {
                    try
                    {
                        transactionScope.Rollback();
                    }
                    catch (Exception ex1)
                    {
                        errorMsg = "Error: Exception occured. " + ex1.StackTrace.ToString();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return errorMsg;
        }
    }

}
