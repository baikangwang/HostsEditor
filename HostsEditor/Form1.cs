using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace HostsEditor
{
    public partial class fmEditor : Form
    {
        // Items data layer
        private ItemsAdpter _itemsAdapter;
        protected ItemsAdpter ItemsAdapter { get { return _itemsAdapter; } }

        // Items data cache
        protected Items ItemsObj { get; set; }

        public fmEditor()
        {
            InitializeComponent();
            this.InitGrid();
            this._itemsAdapter = new ItemsAdpter();
            this.Text = string.Format("HOSTS: {0}", ItemsAdpter.HOSTFILE);
        }

        #region Event
        void fmEditor_Load(object sender, EventArgs e)
        {
            LoadItems();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadItems();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmRefresh_Click(object sender, EventArgs e)
        {
            LoadItems();
        }
        #endregion

        #region GridView Event

        private void dgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                int row = e.RowIndex;
                int col = e.ColumnIndex;

                // When click on Comment cell then do nothing
                if (senderGrid.Rows[row].Cells[col] is CommentsCell) return;

                Item item = this.ItemsObj[row];
                if (item == null) return;

                if (DialogResult.Yes == MessageBox.Show(@"Are you sure to delete this line?", @"Host Deleting", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                {
                    ItemsObj.Remove(item);
                    senderGrid.Rows.RemoveAt(row);
                }
            }
        }

        private void dgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == 0) return;

            var dataGrid = sender as DataGridView;

            //do nothing if it is a new row
            if (dataGrid.Rows[e.RowIndex].IsNewRow) return;

            DataGridViewCell cell = dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string changedValue = cell.Value.ToString();
            this.UpdateItem(e.RowIndex, e.ColumnIndex, changedValue);
        }

        private void dgvItems_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            this.AddItem(string.Empty, string.Empty, string.Empty);
            // add deletebutton
            int rowIndex = this.dgvItems.Rows.IndexOf(e.Row);
            this.dgvItems.Rows[rowIndex - 1].Cells[0].Value = "X";
        }

        #endregion

        #region Methods

        private void InitGrid()
        {
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItems.ColumnHeadersVisible = false;
            dgvItems.AllowUserToOrderColumns = false;
            dgvItems.AllowUserToResizeColumns = false;
            dgvItems.AllowUserToResizeRows = false;

            dgvItems.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing; //or even better .DisableResizing. Most time consumption enum is DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
            // set it to false if not needed
            dgvItems.RowHeadersVisible = false;
            // Double buffering can make DGV slow in remote desktop
            // https://10tec.com/articles/why-datagridview-slow.aspx (Slow DataGridView rendering and scrolling)
            Type dgvType = this.dgvItems.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
              BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgvItems, true, null);

        }

        private void RedrawGrid()
        {
            this.dgvItems.Rows.Clear();
            this.dgvItems.Columns.Clear();

            // var colDelete = new DataGridViewButtonColumn();
            var colIp = new DataGridViewTextBoxColumn();
            var colHost = new DataGridViewTextBoxColumn();
            var colComments = new DataGridViewTextBoxColumn();

            dgvItems.Columns.AddRange(new DataGridViewColumn[] {
            //colDelete,
            colIp,
            colHost,
            colComments});

            // 
            // colDelete
            // 
            //colDelete.HeaderText = "";
            //colDelete.Width = 50;
            // 
            // colIp
            // 
            colIp.HeaderText = "IP";
            colIp.Width = 150;
            // 
            // colHost
            // 
            colHost.HeaderText = "Host";
            colHost.Width = 180;
            // 
            // colComments
            // 
            colComments.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colComments.HeaderText = "Comments";
        }

        private void LoadItems()
        {
            RedrawGrid();

            ItemsObj = ItemsAdapter.Load();
            dgvItems.Rows.Add(ItemsObj.Count);
            for (int i = 0; i < ItemsObj.Count; i++)
            {
                DataGridViewRow row = dgvItems.Rows[i];
                Item item=ItemsObj[i];

                // the cell index will from 1 because the first the column is delete button
                if(item.IsComments)
                {
                    // add comment row if the item is comments
                    int offset = 0;
                    for (int j = offset; j < offset + 3; j++)
                    {
                        CommentsCell commentsCell = new CommentsCell(offset, offset + 2, Color.Green);
                        if (j == offset)
                            commentsCell.Value = item.Comments;
                        row.Cells[j] = commentsCell;
                        commentsCell.ReadOnly = true;
                    }
                }
                else
                {
                    // init delete button
                    //var deleteButton = new DataGridViewButtonCell();
                    //deleteButton.Value = "X";
                    //row.Cells[0] = deleteButton;
                    row.Cells[0].Value = item.IP;
                    row.Cells[1].Value = item.Host;
                    row.Cells[2].Value = item.Comments;
                }
            }
        }

        private void OpenFile()
        {
            // ItemsAdapter.Save(ItemsObj);

            // show the result
            Process.Start("notepad", ItemsAdpter.HOSTFILE);
        }

        private void UpdateItem(int rowIndex,int colIndex,string changedValue)
        {
            int itemIndex = rowIndex;
            int fieldIndex = colIndex;

            Item itemEdited = this.ItemsObj[itemIndex];
            switch (fieldIndex)
            {
                case 1:
                    itemEdited.IP = changedValue;
                    break;
                case 2:
                    itemEdited.Host = changedValue;
                    break;
                case 3:
                    itemEdited.Comments = changedValue;
                    break;
                default:
                    break;
            }
        }

        private void AddItem(string ip,string host,string comments)
        {
            Item item=new Item();
            item.IP = ip;
            item.Host = host;
            item.Comments = comments;
            item.Index = this.ItemsObj.Count;
            this.ItemsObj.Add(item);
        }

        #endregion
    }
}
