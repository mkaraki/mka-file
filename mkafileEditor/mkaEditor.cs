using libmkafile;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace mkafileEditor
{
    public partial class mkaEditor : Form
    {
        private MKAFileDataBuilder builder = null;

        private string filepath = "";

        public mkaEditor()
        {
            InitializeComponent();

            cbox_dtype.Items.AddRange(MKAFileDataBuilder.strDTConverter.Keys.ToArray());
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tbox_appname.Enabled = true;
            tbox_descr.Enabled = true;
            btn_new.Enabled = true;

            string aname = "mkafile Editor";
            builder = new MKAFileDataBuilder(aname);
            tbox_appname.Text = aname;
            tbox_descr.Text = "NO INFORMATION";

            tview_objects.Nodes.Clear();
        }

        private void Btn_new_Click(object sender, EventArgs e)
        {
            string newitemid = RandStr();

            tview_objects.Nodes.Add(newitemid);
            builder.AddObjectHolder(newitemid, MKADataType.empty);
        }

        private string RandStr()
            => Guid.NewGuid().ToString();

        private void Tview_objects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string objname = tview_objects.SelectedNode.Text;

            btn_del.Enabled = true;
            btn_save.Enabled = true;

            tbox_vname.Enabled = true;
            tbox_valued.Enabled = true;
            cbox_dtype.Enabled = true;
            cbox_null.Enabled = true;

            tbox_vname.Text = objname;
            cbox_null.Checked = builder.Objects[objname].IsNull;
            tbox_valued.Enabled = !cbox_null.Checked;
            cbox_dtype.Text = builder.Objects[objname].DataType.ToString();

            if (!cbox_null.Checked)
            {
                dynamic data;
                switch (builder.Objects[objname].DataType)
                {
                    case MKADataType.boolean:
                        data = builder.Objects[objname].RawData;
                        byte rawdata_bool = data[0];
                        switch (rawdata_bool)
                        {
                            case 0x00:
                                tbox_valued.Text = "false";
                                break;

                            case 0x01:
                                tbox_valued.Text = "true";
                                break;

                            default:
                                tbox_valued.Text = "NO DATA";
                                break;
                        }
                        break;

                    case MKADataType.binary:
                        tbox_valued.Text = Convert.ToBase64String(builder.Objects[objname].RawData);
                        break;

                    case MKADataType.int32:
                        data = BitConverter.ToInt32(builder.Objects[objname].RawData, 0);
                        tbox_valued.Text = data.ToString();
                        break;

                    case MKADataType.uint32:
                        data = BitConverter.ToUInt32(builder.Objects[objname].RawData, 0);
                        tbox_valued.Text = data.ToString();
                        break;

                    case MKADataType.int64:
                        data = BitConverter.ToInt64(builder.Objects[objname].RawData, 0);
                        tbox_valued.Text = data.ToString();
                        break;

                    case MKADataType.uint64:
                        data = BitConverter.ToUInt64(builder.Objects[objname].RawData, 0);
                        tbox_valued.Text = data.ToString();
                        break;

                    case MKADataType.utf8:
                        data = Encoding.UTF8.GetString(builder.Objects[objname].RawData);
                        tbox_valued.Text = data;
                        break;
                }
            }
            else
                tbox_valued.Text = string.Empty;
        }

        private void Cbox_null_CheckedChanged(object sender, EventArgs e)
        {
            if (cbox_null.Checked)
                tbox_valued.Enabled = false;
            else
                tbox_valued.Enabled = true;
        }

        private void Btn_del_Click(object sender, EventArgs e)
        {
            string objname = tview_objects.SelectedNode.Text;
            builder.RemoveObjectHolder(objname);

            tview_objects.SelectedNode.Remove();
        }

        private void Btn_save_Click(object sender, EventArgs e)
        {
            if (tview_objects.SelectedNode.Text != tbox_vname.Text)
            {
                builder.RenameObjectHolder(tview_objects.SelectedNode.Text, tbox_vname.Text);
                tview_objects.SelectedNode.Text = tbox_vname.Text;
            }

            string vname = tbox_vname.Text;

            builder.ChangeDataType(vname, cbox_dtype.Text);

            if (cbox_null.Checked)
                builder.SetNull(vname);
            else
            {
                dynamic data;

                switch (cbox_dtype.Text)
                {
                    case "empty":
                        builder.SetNull(vname);
                        break;

                    case "boolean":
                        data = new byte[1];
                        switch (tbox_valued.Text)
                        {
                            case "true": data[0] = 0x01; break;
                            case "false": data[0] = 0x00; break;
                            default: throw new Exception("Not Allowed Value");
                        }
                        builder.SetBytes(vname, data);
                        break;

                    case "int32":
                        data = int.Parse(tbox_valued.Text);
                        builder.SetBytes(vname, BitConverter.GetBytes(data));
                        break;

                    case "uint32":
                        data = uint.Parse(tbox_valued.Text);
                        builder.SetBytes(vname, BitConverter.GetBytes(data));
                        break;

                    case "int64":
                        data = long.Parse(tbox_valued.Text);
                        builder.SetBytes(vname, BitConverter.GetBytes(data));
                        break;

                    case "uint64":
                        data = ulong.Parse(tbox_valued.Text);
                        builder.SetBytes(vname, BitConverter.GetBytes(data));
                        break;

                    case "binary":
                        data = Convert.FromBase64String(tbox_valued.Text);
                        builder.SetBytes(vname, data);
                        break;

                    case "utf8":
                        data = Encoding.UTF8.GetBytes(tbox_valued.Text);
                        builder.SetBytes(vname, data);
                        break;
                }
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;

                using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                {
                    var data = MKAFileConverter.DeserializeFromStream(fs);

                    tbox_appname.Enabled = true;
                    tbox_descr.Enabled = true;
                    btn_new.Enabled = true;

                    builder = new MKAFileDataBuilder(data);
                    tbox_appname.Text = data.ApplicationName;
                    tbox_descr.Text = data.Description;

                    tview_objects.Nodes.Clear();
                    foreach (var d in data.Objects)
                        tview_objects.Nodes.Add(d.Key);
                }
            }

            ofd.Dispose();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (builder == null) return;

            var sfd = new SaveFileDialog();
            sfd.FileName = filepath;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (builder.ApplicationName != tbox_appname.Text)
                    builder.ApplicationName = tbox_appname.Text;

                if (builder.Description != tbox_descr.Text)
                    builder.Description = tbox_descr.Text;

                using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                {
                    MKAFileConverter.SerializeToStream(builder.Build(), fs);
                }
            }

            sfd.Dispose();
        }
    }
}