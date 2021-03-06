﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ePlusReplication
{
    
    public partial class Form1 : Form
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        public static long Fetchvalue, Applyvalue, Verifyvalue;
        clsXMLData clsxml = new clsXMLData();
        public Boolean mainDBConnection;
        protected DatabaseInfo MainDBInfo, LogDBInfo;
        protected DatabaseInfo[] slaveDatabaseInfo;
        public MySqlConnection MainDBConne, slaveDatabase, LogDBConne;
        public string dbCode,errorMessage;
        public string[] Error;
       // protected xmlSettings settings;
        protected eMailSettings testMail;    
        protected System.Collections.Queue replicationQueue;
        protected long count;
        clsDBConnect dbconnect;
        public static string str,brnch;
        delegate void StringParameterDelegate(string Text);
        delegate void StringClearParameterDelegate();
        delegate void SplashShowCloseDelegate();
         bool CloseSplashScreenFlag = false;
        DataTable dt = new DataTable();
        private xmlSettings xmlsettings;
        public ulong replicationID;
      //  NotifyIcon notify = new NotifyIcon();



        public Form1()
        {
           
            InitializeComponent();
        }       
        public void ShowScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(ShowScreen));
                return;
            }
            this.Show();
            Application.Run(this);
        }
        /// Closes the SplashScreen
        /// </summary>
        public void CloseScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(CloseScreen));
                return;
            }
            CloseSplashScreenFlag = true;
            this.Close();
        }
        /// Update text in default green color of success message
        /// </summary>
        /// <param name="Text">Message</param>
        public void UdpateStatusText(string Text)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterDelegate(UdpateStatusText), new object[] { Text });
                return;
            }
            // Must be on the UI thread if we've got this far
            //label1.ForeColor = Color.Green;

            txtStatus.Text += Text;
            str = txtStatus.Text;           
        }
        public void ClearStatusText()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringClearParameterDelegate(ClearStatusText));
                return;
            }

            txtStatus.Text = "";
        }
        /// <summary>
        /// Prevents the closing of form other than by calling the CloseSplashScreen function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplashForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseSplashScreenFlag == false)
                e.Cancel = true;
        }       
         
        private void dgview_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridView gdview = sender as DataGridView;
            if(null!=gdview)
            {
                foreach(DataGridViewRow r in gdview.Rows)
                {
                    gdview.Rows[r.Index].HeaderCell.Value = (r.Index+1).ToString();
                }
            }
        }

        private void dgview_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

       private void Form1_Resize(object sender, EventArgs e)
        {
            if(WindowState==FormWindowState.Minimized)
            {
                ProcessIcon.flag = true;
                this.ShowInTaskbar = true;
                notifyIcon.Visible = true;
                this.Visible = true;
            }
         
        }

        private void dgview_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = 0;
            dgview.Rows[dgview.SelectedCells[i].RowIndex].Selected = true;
            //Error[i] = clsDBUtility.error;
            int index = e.RowIndex;
            txtDetails.Enabled = true;
            txtDetails.Text = "Next Replication:" + frmMain.nxtTime + "\r\n"+ "Branch :" + clsReplicate.BranchName[index];
            if(clsDBUtility.error !="")
             txtDetails.Text += "\r\nPending:" + clsDBUtility.error + "\r\n";
            i++;
            
        }

        private void dgview_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (MessageBox.Show(clsReplicate.pendingquery, "Pending Query", MessageBoxButtons.RetryCancel, MessageBoxIcon.None) == DialogResult.Retry)
            {
                xmlsettings = clsxml.ReadSettings();
                dbconnect = new clsDBConnect(xmlsettings);
                dbconnect.Requeue(dbconnect.LogDBConne, ref errorMessage);
                dbconnect.LogDBConne.Close();
                dbconnect.LogDBConne.Dispose();

            }
            else
            {
                xmlsettings = clsxml.ReadSettings();
                dbconnect = new clsDBConnect(xmlsettings);
                dbconnect.UpdateSkip(dbconnect.LogDBConne, ref errorMessage);
                dbconnect.LogDBConne.Close();
                dbconnect.LogDBConne.Dispose();
            }

        }

        private void updTimer_Tick(object sender, EventArgs e)
        {
            int size = clsReplicate.BranchName.Length;
            updateGridView(size);
        }
        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
        private void btnError_Click(object sender, EventArgs e)
        {            
            if(MessageBox.Show(clsReplicate.pendingquery,"Pending Query",MessageBoxButtons.RetryCancel,MessageBoxIcon.None)==DialogResult.Retry)
            {               
                xmlsettings = clsxml.ReadSettings();
                dbconnect = new clsDBConnect(xmlsettings);
                dbconnect.Requeue(dbconnect.LogDBConne, ref errorMessage);
                dbconnect.LogDBConne.Close();
                dbconnect.LogDBConne.Dispose();

            }
            else
            {               
                xmlsettings = clsxml.ReadSettings();
                dbconnect = new clsDBConnect(xmlsettings);
                dbconnect.UpdateSkip(dbconnect.LogDBConne, ref errorMessage);
                dbconnect.LogDBConne.Close();
                dbconnect.LogDBConne.Dispose();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {          
            dgview.Cursor = Cursors.Arrow;
            txtStatus.Cursor = Cursors.No;
            txtDetails.Cursor = Cursors.Arrow;  
        }
        private void dgview_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
      /*      Form1 frm = new Form1();
            clsxml.ReadSettings();   
            e.Row.Cells["Branch"].Value = "BR1";
            e.Row.Cells["LastReplication"].Value =DateTime.Now ;
            e.Row.Cells["FetchedQuery"].Value = Fetchvalue;
            e.Row.Cells["VerifiedQuery"].Value = Verifyvalue;
            e.Row.Cells["AppliedQuery"].Value = Applyvalue;
            e.Row.Cells["NextReplication"].Value = "__/__/__  __:__:__";
            e.Row.Cells["Status"].Value = "";      */
        }             

       public void updateGridView(int size)

        {
            
            DataTable dt = new DataTable();
            
            dt.Columns.Add("Branch", typeof(string));
            dt.Columns.Add("LastReplication", typeof(DateTime));
            dt.Columns.Add("FetchedQuery", typeof(long));
            dt.Columns.Add("VerifiedQuery", typeof(long));
            dt.Columns.Add("AppliedQuery", typeof(long));
            dt.Columns.Add("NextReplication", typeof(DateTime));
            dt.Columns.Add("Status", typeof(string));
            /*   if(clsReplicate.branchName=="SDKServer")
               {
                   brnch = "HO";
               }
               else if(clsReplicate.branchName=="SDK4-PC")
               {
                   brnch = "NALO";
               }
               else
               {
                   brnch = "BR1";
               }     */
            /*  DataTable dt2 = new DataTable();
              dt2 = clsReplicate.branchName;
              // dt2.Columns.Add("Branch", typeof(string));
              dt2.Rows.Add();
              dgview.DataSource = dt2;       */
            for (int i = 0; i < size; i++)
            {
                dt.Rows.Add(clsReplicate.BranchName[i], frmMain.lstTime, clsReplicate.Fetchvalue, clsReplicate.Verifyvalue, clsReplicate.pendingcount, frmMain.nxtTime);
                if (clsDBUtility.error=="")
                {
                    dt.Rows[i]["Status"] = "Success";

                }
                else
                {
                    dt.Rows[i]["Status"] = "Pending";
                    

                }
              dgview.DataSource = dt;
            }

           

         //   DataTable dtmerged = MergeTables(dt,dt2);
        /*    dt2.Merge(dt);
            dt2.AcceptChanges();       */
        }
    }
}
