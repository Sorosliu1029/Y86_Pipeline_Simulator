using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Timers;

/******************************************************     
 * FileName:  Pipe.cs
 * Writer:  Liu Yang
 * create Date: 2015-6-5
 * Main Content: 
 * the UI for the Pipeline presentation
 *******************************************************/

namespace Y86_Pipeline_Simulator
{
    public partial class FrmMain : Form
    {
        FrmAssembler ob_FrmAssembler;
        Pipe pp = new Pipe();
        System.Timers.Timer timer = new System.Timers.Timer();          //auto complete timer
        Thread thread_update_UI;
        //document read path
        string InputFilePath;
        string OutputFilePath;
        string pipeline_regs_record;
        string reg_file_record;
        string cc_record;
        string Y86_instr_file;
        string cmb_logic_val_record;
        string pipeline_control_record;
        string stack_record;

        int current_line;       //a line is for a cycle
        int line_length;
        string[] regs = { "%eax", "%ecx", "%edx", "%ebx", "%esp", "%ebp", "%esi", "%edi", "RNONE" };
        string[] stats = { "Null", "AOK", "HLT", "ADR", "INS", "BUB" };
        Color[] pipeline_regs_bgcolors= { Color.LightSalmon, Color.Gold, Color.GreenYellow, Color.LightSteelBlue, Color.Violet };
        //load the all values that need to show in the UI
        string[] pipeline_regs_record_lines;
        string[] prr_val;
        string[] reg_file_record_lines;
        string[] rfr_val;
        string[] cc_record_lines;
        string[] cc_val;
        string[] cmb_logic_val_record_lines;
        string[] clvr_val;
        string[] pipeline_control_record_lines;
        string[] pcr_val;
        string[] assembled_Y86_instr_file;
        string[] stack_record_lines;
        string[] sr_val;
        string[] stack_esp_ebp;
        string[] temp_val;
        bool[] changed;

        double timer_interval;
        bool end_messageBox_show;
        int Y86_instr_selected_line;
        string break_point_imem_addr;
        public FrmMain()
        {
            InitializeComponent();
        }
        void objClose()
        {
            rdbtnL.Enabled = rdbtnM.Enabled = rdbtnH.Enabled = false;
            btnOneStep.Enabled = false;
            btnPause.Enabled = false;
            btnAutoRun.Enabled = false;
            btnReset.Enabled = false;
            numPeriod.Enabled = false;
            btnGo.Enabled = btnBack.Enabled = false;
            txtStartAddr.Enabled = numLength.Enabled = false;
            btnMemOk.Enabled = false;
            btnGo2BreakPoint.Enabled = false;
            pbPeriod.Enabled = false;
        }
        void objOpen()
        {
            rdbtnL.Enabled = rdbtnM.Enabled = rdbtnH.Enabled = true;
            btnOneStep.Enabled = true;
            btnReset.Enabled = true;
            numPeriod.Enabled = true;
            btnGo.Enabled = btnBack.Enabled = true;
            txtStartAddr.Enabled = numLength.Enabled = true;
            btnMemOk.Enabled = true;
            pbPeriod.Enabled = true;
        }

        //used to create the pipeline flowing effect
        void pipeline_regs_bgcolor_change()
        {
            lblF_predPC.BackColor = pipeline_regs_bgcolors[current_line % 5];
            lblD_icode.BackColor = lblD_ifun.BackColor = lblD_rA.BackColor = lblD_rB.BackColor =
                lblD_stat.BackColor = lblD_valC.BackColor = lblD_valP.BackColor = pipeline_regs_bgcolors[(current_line +4) % 5];
            lblE_stat.BackColor = lblE_icode.BackColor = lblE_ifun.BackColor = lblE_valC.BackColor =
                lblE_valA.BackColor = lblE_valB.BackColor = lblE_dstE.BackColor = lblE_dstM.BackColor =
                lblE_srcA.BackColor = lblE_srcB.BackColor = pipeline_regs_bgcolors[(current_line +3) % 5];
            lblM_stat.BackColor = lblM_icode.BackColor = lblM_Cnd.BackColor = lblM_valE.BackColor =
                lblM_valA.BackColor = lblM_dstE.BackColor = lblM_dstM.BackColor = pipeline_regs_bgcolors[(current_line +2) % 5];
            lblW_stat.BackColor = lblW_icode.BackColor = lblW_valE.BackColor = lblW_valM.BackColor =
                lblW_dstE.BackColor = lblW_dstM.BackColor = pipeline_regs_bgcolors[(current_line +1) % 5];
        }

        //highlight the register file that have benn changed compared to the last cycle
        void reg_file_bgcolor_change()
        {
            temp_val = reg_file_record_lines[current_line - 1].Split(',');
            changed= new bool[8];
            for (int i = 0; i < 8; i++)
            {
                changed[i] = (rfr_val[i] == temp_val[i]);
            }
            txteax.BackColor = changed[0] ? Color.Moccasin : Color.PaleTurquoise;
            txtecx.BackColor = changed[1] ? Color.Moccasin : Color.PaleTurquoise;
            txtedx.BackColor = changed[2] ? Color.Moccasin : Color.PaleTurquoise;
            txtebx.BackColor = changed[3] ? Color.Moccasin : Color.PaleTurquoise;
            txtesp.BackColor = changed[4] ? Color.Moccasin : Color.PaleTurquoise;
            txtebp.BackColor = changed[5] ? Color.Moccasin : Color.PaleTurquoise;
            txtesi.BackColor = changed[6] ? Color.Moccasin : Color.PaleTurquoise;
            txtedi.BackColor = changed[7] ? Color.Moccasin : Color.PaleTurquoise;
        }

        //highlight the condition code that have been changed compared to the last cycle
        void cc_bgcolor_change()
        {
            temp_val = cc_record_lines[current_line - 1].Split(',');
            changed = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                changed[i] = (cc_val[i] == temp_val[i]);
            }
            txtZF.BackColor = changed[0] ? Color.Moccasin : Color.PaleTurquoise;
            txtSF.BackColor = changed[1] ? Color.Moccasin : Color.PaleTurquoise; ;
            txtOF.BackColor = changed[2] ? Color.Moccasin : Color.PaleTurquoise;
        }
        Color select_cmb_condition_logic_bgcolor(bool selected)
        {
            return selected ? Color.LawnGreen : Color.Red;
        }
        Color select_pipeline_control_bgcolor(string a)
        {
            bool selected = Convert.ToBoolean(a);
            return selected ? Color.LightSkyBlue : Color.Silver;
        }
        void read_assembled_Y86_instr_file(string readPath)
        {
            try
            {
                assembled_Y86_instr_file = File.ReadAllLines(readPath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_pipeline_regs_record(string readPath)
        {
            try
            {
                pipeline_regs_record_lines= File.ReadAllLines(readPath, Encoding.Default);          
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void show_pipeline_regs_val()
        {
            try
            {                
                prr_val = pipeline_regs_record_lines[current_line].Split(',');
                lblF_predPC.Text = "0x" + prr_val[0]; lblD_icode.Text = prr_val[2]; lblD_ifun.Text = prr_val[3]; lblD_valC.Text = "0x" + prr_val[6];
                lblD_valP.Text = "0x" + prr_val[7]; lblE_icode.Text = prr_val[9]; lblE_ifun.Text = prr_val[10]; lblE_valC.Text = "0x" + prr_val[11];
                lblE_valA.Text = "0x" + prr_val[12]; lblE_valB.Text = "0x" + prr_val[13]; lblM_icode.Text = prr_val[19]; lblM_valE.Text = "0x" + prr_val[21];
                lblM_valA.Text = "0x" + prr_val[22]; lblW_icode.Text = prr_val[26]; lblW_valE.Text = "0x" + prr_val[27]; lblW_valM.Text = "0x" + prr_val[28];
                lblD_rA.Text = regs[Convert.ToInt32(prr_val[4])]; lblD_rB.Text = regs[Convert.ToInt32(prr_val[5])];
                lblE_dstE.Text = regs[Convert.ToInt32(prr_val[14])]; lblE_dstM.Text = regs[Convert.ToInt32(prr_val[15])];
                lblE_srcA.Text = regs[Convert.ToInt32(prr_val[16])]; lblE_srcB.Text = regs[Convert.ToInt32(prr_val[17])];
                lblM_dstE.Text = regs[Convert.ToInt32(prr_val[23])]; lblM_dstM.Text = regs[Convert.ToInt32(prr_val[24])];
                lblW_dstE.Text = regs[Convert.ToInt32(prr_val[29])]; lblW_dstM.Text = regs[Convert.ToInt32(prr_val[30])];
                lblD_stat.Text = stats[Convert.ToInt32(prr_val[1])]; lblE_stat.Text = stats[Convert.ToInt32(prr_val[8])];
                lblM_stat.Text = stats[Convert.ToInt32(prr_val[18])]; lblW_stat.Text = stats[Convert.ToInt32(prr_val[25])];
                lblM_Cnd.Text = prr_val[20];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_reg_file_record(string readPath)
        {
            try
            {
                reg_file_record_lines = File.ReadAllLines(readPath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void show_reg_file_val()
        {
            try
            {
                rfr_val = reg_file_record_lines[current_line].Split(',');
                txteax.Text = "0x" + rfr_val[0]; txtecx.Text = "0x" + rfr_val[1];
                txtedx.Text = "0x" + rfr_val[2]; txtebx.Text = "0x" + rfr_val[3];
                txtesp.Text = "0x" + rfr_val[4]; txtebp.Text = "0x" + rfr_val[5];
                txtesi.Text = "0x" + rfr_val[6]; txtedi.Text = "0x" + rfr_val[7];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_cc_record(string readPath)
        {
            try
            {
                cc_record_lines = File.ReadAllLines(readPath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void show_cc_val()
        {
            try
            {
                cc_val = cc_record_lines[current_line].Split(',');
                txtZF.Text = cc_val[0]; txtSF.Text = cc_val[1]; txtOF.Text = cc_val[2];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_pipeline_control_record(string readPath)
        {
            try
            {
                pipeline_control_record_lines = File.ReadAllLines(readPath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void show_pipeline_control_val()
        {
            try
            {
                pcr_val = pipeline_control_record_lines[current_line].Split(',');
                txtF_stall.BackColor = select_pipeline_control_bgcolor(pcr_val[0]);
                txtF_bubble.BackColor = select_pipeline_control_bgcolor(pcr_val[1]);
                txtD_stall.BackColor = select_pipeline_control_bgcolor(pcr_val[2]);
                txtD_bubble.BackColor = select_pipeline_control_bgcolor(pcr_val[3]);
                txtE_stall.BackColor = select_pipeline_control_bgcolor(pcr_val[4]);
                txtE_bubble.BackColor = select_pipeline_control_bgcolor(pcr_val[5]);
                txtM_stall.BackColor = select_pipeline_control_bgcolor(pcr_val[6]);
                txtM_bubble.BackColor = select_pipeline_control_bgcolor(pcr_val[7]);
                txtW_stall.BackColor = select_pipeline_control_bgcolor(pcr_val[8]);
                txtW_bubble.BackColor = select_pipeline_control_bgcolor(pcr_val[9]);                
                txtFwd_A.Text=pcr_val[10];
                txtFwd_B.Text=pcr_val[11];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_cmb_logic_val_record(string readPath)
        {
            try
            {
                cmb_logic_val_record_lines = File.ReadAllLines(readPath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void show_cmb_logic_val()
        {
            try
            {
                clvr_val = cmb_logic_val_record_lines[current_line].Split(',');
                lblf_pc.Text = "0x" + clvr_val[0];
                lblM_valA1.Text = lblM_valA2.Text = "0x" + clvr_val[1];
                lblW_valM1.Text = lblW_valM2.Text = lblW_valM3.Text = "0x"+clvr_val[2];
                lblW_valE1.Text = lblW_valE2.Text ="0x"+ clvr_val[5];
                lbld_srcA.Text = regs[Convert.ToInt32(clvr_val[6])];
                lbld_srcB.Text = regs[Convert.ToInt32(clvr_val[7])];
                lble_dstE1.Text = regs[Convert.ToInt32(clvr_val[8])];
                lble_Cnd.Text = clvr_val[9];
                lblM_Cnd1.Text = clvr_val[10];
                lblM_valE1.Text = "0x" + clvr_val[11];
                lblm_stat1.Text = stats[Convert.ToInt32(clvr_val[14])];
                lblm_valM.Text = "0x" + clvr_val[16];
                lblStat.Text = stats[Convert.ToInt32(clvr_val[17])];
                lblimem_error.BackColor = select_cmb_condition_logic_bgcolor( ! Convert.ToBoolean(clvr_val[3]));
                lblinstr_valid.BackColor = select_cmb_condition_logic_bgcolor(Convert.ToBoolean(clvr_val[4]));
                lblread.BackColor = select_cmb_condition_logic_bgcolor(Convert.ToBoolean(clvr_val[12]));
                lblwrite.BackColor = select_cmb_condition_logic_bgcolor(Convert.ToBoolean(clvr_val[13]));
                lbldmem_error.BackColor = select_cmb_condition_logic_bgcolor( ! Convert.ToBoolean(clvr_val[15]));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_stack_record(string readPath)
        {
            try
            {
                stack_record_lines = File.ReadAllLines(readPath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void show_stack_info()
        {
            try
            {
                if (current_line >= 5)
                {
                    sr_val = stack_record_lines[current_line].Split(',');
                    lvStack.View = View.Details;
                    stack_esp_ebp = sr_val[0].Split('&');
                    int ini_stack = Convert.ToInt32(stack_esp_ebp[0]);
                    int esp = Convert.ToInt32(stack_esp_ebp[1]);
                    int ebp = Convert.ToInt32(stack_esp_ebp[2]);
                    ListViewItem a_line;
                    lvStack.Items.Clear();
                    for (int i = ini_stack, j = 1; i >= esp; i -= 4, j++)
                    {
                        a_line = new ListViewItem("0x" + i.ToString("x3"));
                        a_line.SubItems.Add("0x"+sr_val[j]);
                        if (i == ebp && i == esp)
                        {
                            a_line.SubItems.Add("<==%esp,%ebp");
                        }
                        else if (i == ebp || i == esp)
                        {
                            if (i == ebp)
                            {
                                a_line.SubItems.Add("<== %ebp");
                            }
                            if (i == esp)
                            {
                                a_line.SubItems.Add("<== %esp");
                            }
                        }                        
                        else a_line.SubItems.Add(" ");
                        lvStack.Items.Add(a_line);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void read_Y86_instr(string readPath)
        {
            try
            {
                lvY86_instr.View = View.Details;
                ListViewItem a_line;
                string line;
                int line_num = 1;
                StreamReader sr = new StreamReader(readPath, Encoding.Default);
                lvY86_instr.Items.Clear();
                while ((line = sr.ReadLine()) != null)
                {
                    a_line = new ListViewItem(line_num.ToString());
                    a_line.SubItems.Add(line);
                    lvY86_instr.Items.Add(a_line);
                    line_num++;
                }
                sr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnInputFile_Click(object sender, EventArgs e)
        {
            try
            {
                odlgInputFile.FilterIndex = 0;
                odlgInputFile.FileName = "";
                odlgInputFile.Filter = "Yo File (*.yo)|*.yo|All Files (*.*)|*.*";
                if (odlgInputFile.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = odlgInputFile.FileName.ToString();
                    txtInputFile.ReadOnly = true;
                }
                InputFilePath = txtInputFile.Text.Trim();
                txtOutputFile.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnOutputFile_Click(object sender, EventArgs e)
        {
            try
            {
                sdlgOutputFile.FilterIndex = 0;
                sdlgOutputFile.FileName = "";
                sdlgOutputFile.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";
                if (sdlgOutputFile.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = sdlgOutputFile.FileName.ToString();
                    txtOutputFile.ReadOnly = true;
                }
                OutputFilePath = txtOutputFile.Text.Trim();
                //Prepare
                if (File.Exists(pipeline_regs_record))
                {
                    File.Delete(pipeline_regs_record);
                }
                if (File.Exists(reg_file_record))
                {
                    File.Delete(reg_file_record);
                }
                if (File.Exists(cc_record))
                {
                    File.Delete(cc_record);
                }
                if (File.Exists(Y86_instr_file))
                {
                    File.Delete(Y86_instr_file);
                }
                if (File.Exists(cmb_logic_val_record))
                {
                    File.Delete(cmb_logic_val_record);
                }
                if (File.Exists(stack_record))
                {
                    File.Delete(stack_record);
                }
                if (File.Exists(pipeline_control_record))
                {
                    File.Delete(pipeline_control_record);
                }
                objOpen();
                pp.ini_Pipe();
                //auto complete all instructions in the background
                pp.complete_pipeline(InputFilePath, OutputFilePath);
                //get all values that need to show from .txt document
                read_Y86_instr(Y86_instr_file);
                read_assembled_Y86_instr_file(InputFilePath);
                read_pipeline_regs_record(pipeline_regs_record);                
                read_reg_file_record(reg_file_record);
                read_cc_record(cc_record);
                read_pipeline_control_record(pipeline_control_record);
                read_cmb_logic_val_record(cmb_logic_val_record);
                read_stack_record(stack_record);

                line_length = pipeline_regs_record_lines.Length;
                pbPeriod.Maximum = line_length;
                current_line = 0;
                end_messageBox_show = false;        
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {
                pp.writeDirectory = Application.StartupPath;
                pipeline_regs_record = Application.StartupPath + "\\" + "pipeline_regs_record.txt";
                reg_file_record = Application.StartupPath + "\\" + "reg_file_record.txt";
                cc_record = Application.StartupPath + "\\" + "cc_record.txt";
                Y86_instr_file = Application.StartupPath + "\\" + "Y86_instr.txt";
                cmb_logic_val_record = Application.StartupPath + "\\" + "cmb_logic_val_record.txt";
                pipeline_control_record = Application.StartupPath + "\\" + "pipeline_control_record.txt";
                stack_record = Application.StartupPath + "\\" + "stack_record.txt";
                timer.Elapsed += new ElapsedEventHandler(btnOneStep_Click);
                objClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void update_UI()
        {
            txtPeriod.Text = current_line.ToString();
            if (current_line<pbPeriod.Maximum)
            {
                pbPeriod.Value = current_line+1;
            }
            show_pipeline_regs_val();
            show_reg_file_val();
            show_cc_val();
            show_pipeline_control_val();
            show_cmb_logic_val();
            show_stack_info();            
            pipeline_regs_bgcolor_change();
            if (current_line >= 1)
            {
                reg_file_bgcolor_change();
                cc_bgcolor_change();
            }
            current_line++;
        }
        public delegate void MyInvoke();
        void update()
        {
            MyInvoke mi = new MyInvoke(update_UI);          
            this.BeginInvoke(mi, null);
        }
        private void btnOneStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (current_line >= line_length)
                {
                    timer.Enabled = false;
                    thread_update_UI.Abort();           //stop the thread
                    if (!end_messageBox_show)
                    {
                        MessageBox.Show("程序已结束!", "提醒", MessageBoxButtons.OK);                         
                        end_messageBox_show = true;                        
                    }                
                    return;
                }
                thread_update_UI = new Thread(new ThreadStart(update));         //create a mutil_thread
                thread_update_UI.Start();           //start the thread
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //used for looking up the memory information
        string read_mem(byte[] a, int length)
        {
            byte[] n = new byte[8];
            for (int i = 0; i < 8; i++)
                n[i] = 0;
            for (int i = 0; i < 4; i++)
                pp.decompose(ref n[2 * i], ref n[2 * i + 1], a[3-i]);
            string result = "";
            for (int i = 0; i < 2*length; i++)
                result += n[i].ToString("x");
            return result;
        }
        private void btnMemOk_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] mem_val = new byte[4];
                int start_Addr = Convert.ToInt32(txtStartAddr.Text.Trim());
                for (int i = 0; i < 4; i++)
                {
                    mem_val[i] = pp.mem[start_Addr + i];
                }
                txtMemValue.Text = "0x" + read_mem(mem_val, Convert.ToInt32(numLength.Value));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                int cycle_num = Convert.ToInt32(numPeriod.Value);
                if (current_line -1+ cycle_num >= line_length)
                {
                    MessageBox.Show("   程序已结束!\n请适当减小周期数。", "提醒", MessageBoxButtons.OK);
                    return;
                }
                current_line -= 1;
                current_line+= cycle_num;
                btnOneStep_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                int cycle_num = Convert.ToInt32(numPeriod.Value);
                if (current_line-1 - cycle_num< 0)
                {
                    MessageBox.Show("回退周期数过多!", "提醒", MessageBoxButtons.OK);
                    return;
                }
                current_line -= 1;
                current_line -= cycle_num;
                btnOneStep_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                thread_update_UI.Abort();           //stop the thread
                timer.Stop();
                timer.Enabled = false;
                btnPause.Text = "暂停";
                btnPause.Enabled = false;
                btnOneStep.Enabled = true;
                current_line = 0;
                end_messageBox_show = false;
                btnOneStep_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnAutoRun_Click(object sender, EventArgs e)
        {
            try
            {
                if (timer.Enabled)
                {
                    return;
                }
                btnPause.Text = "暂停";
                btnPause.Enabled = true;
                btnOneStep.Enabled = false;
                timer.Interval = timer_interval;
                timer.Start();
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void rdbtnL_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                timer_interval = 1000/Convert.ToDouble(rdbtnL.Text);
                btnAutoRun.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void rdbtnM_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                timer_interval = 1000/Convert.ToDouble(rdbtnM.Text);
                btnAutoRun.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void rdbtnH_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                timer_interval = 1000/Convert.ToDouble(rdbtnH.Text);
                btnAutoRun.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnPause.Text == "暂停")
                {
                    btnPause.Text = "继续";
                    timer.Stop();
                    timer.Enabled = false;
                    btnOneStep.Enabled = true;
                }
                else
                {
                    btnPause.Text = "暂停";
                    timer.Interval = timer_interval;
                    timer.Start();
                    timer.Enabled = true;
                    btnOneStep.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void lvY86_instr_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            try
            {
                e.NewWidth = lvY86_instr.Columns[e.ColumnIndex].Width;
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        bool get_Y86_imem_addr(string instr_line, ref string imem_addr)
        {
            int i = instr_line.IndexOf('|');
            instr_line = instr_line.Substring(0, i).Trim();
            if (instr_line != "")
            {
                imem_addr = instr_line;
                return true;
            }
            else
                return false;
        }

        //get the memory address of the instruction where the break point is set
        private void lvY86_instr_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (lvY86_instr.SelectedIndices != null && lvY86_instr.SelectedIndices.Count > 0)
                {
                    btnGo2BreakPoint.Enabled = true;
                    Y86_instr_selected_line = lvY86_instr.SelectedIndices[0];
                    txtBreak_Point.Text = "" + (Y86_instr_selected_line+1);
                    while (! get_Y86_imem_addr(assembled_Y86_instr_file[Y86_instr_selected_line], ref break_point_imem_addr))
                    {
                        Y86_instr_selected_line++;
                    }
                    int i = break_point_imem_addr.IndexOf(':');
                    break_point_imem_addr = break_point_imem_addr.Substring(2, i-2);
                    break_point_imem_addr = Convert.ToInt32(break_point_imem_addr,16).ToString("x8");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnGo2BreakPoint_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < cmb_logic_val_record_lines.Length; i++)
                {
                    temp_val = cmb_logic_val_record_lines[i].Split(',');
                    if (break_point_imem_addr ==temp_val[0])
                    {
                        current_line = i;
                    }
                }
                btnOneStep_Click(sender, e);
                btnGo2BreakPoint.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnAssembler_Click(object sender, EventArgs e)
        {
            if (ob_FrmAssembler == null || ob_FrmAssembler.IsDisposed)
            {
                ob_FrmAssembler = new FrmAssembler();
                ob_FrmAssembler.Show();
            }
            else
            {
                ob_FrmAssembler.Activate();
            }
        }
        private void llblContact_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("mailto:13307130167@fudan.edu.cn");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lvStack_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            try
            {
                e.NewWidth = lvStack.Columns[e.ColumnIndex].Width;
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
