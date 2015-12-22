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

/******************************************************     
 * FileName:  FrmAssembler.cs
 * Writer:  Liu Yang
 * create Date: 2015-6-5
 * Main Content: 
 * the UI for the assembler
 *******************************************************/

namespace Y86_Pipeline_Simulator
{
    public partial class FrmAssembler : Form
    {
        Assembler ass = new Assembler();
        string InputFilePath;
        string OutputFilePath;
        string[] code_to_assemble;
        string[] code_to_disassemble;
        int instr_addr;
        public FrmAssembler()
        {
            InitializeComponent();
        }
        void read_code_to_assemble()
        {
            code_to_assemble.Initialize();
            code_to_assemble = rtbxInput.Text.Split('\n');
        }
        void read_code_to_disassemle()
        {
            code_to_disassemble.Initialize();
            code_to_disassemble = rtbxInput.Text.Split('\n');
        }
        private void btnInputFile_Click(object sender, EventArgs e)
        {
            try
            {
                odlgInput.FilterIndex = 0;
                odlgInput.FileName = "";
                odlgInput.Filter = "text File (*.txt)|*.txt|All Files (*.*)|*.*";
                if (odlgInput.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = odlgInput.FileName.ToString();
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
                sdlgOutput.FilterIndex = 0;
                sdlgOutput.FileName = "";
                sdlgOutput.Filter = "Yo File (*.yo)|*.yo|All Files (*.*)|*.*";
                if (sdlgOutput.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = sdlgOutput.FileName.ToString();
                    txtOutputFile.ReadOnly = true;
                }
                OutputFilePath = txtOutputFile.Text.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        string fullfil_blank(int num)
        {
            string a = "";
            for (int i = 0; i < 13 - num; i++)
            {
                a += " ";
            }
            return a;
        }
        private void btnAssemble_Click(object sender, EventArgs e)
        {
            try
            {
                if (rtbxInput.Text.Trim() == "")
                {
                    MessageBox.Show("请输入!", "提醒");
                    return;
                }
                read_code_to_assemble();        //split each line into a string array
                rtbxOutput.Text = "";
                instr_addr = 0;
                for (int i = 0; i < code_to_assemble.Length; i++)
                {
                    if (code_to_assemble[i].Trim() == "")       //blank line
                    {
                        rtbxOutput.AppendText("\n");
                        continue;
                    }
                    string p1 = ass.assemble(code_to_assemble[i].Trim() + " ");         //get assembled code
                    rtbxOutput.AppendText("  0x" + instr_addr.ToString("x3") + ": " + p1 + fullfil_blank(p1.Length) + "| " + code_to_assemble[i].Trim());
                    rtbxOutput.AppendText("\n");
                    instr_addr += (p1.Length / 2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnDisassembler_Click(object sender, EventArgs e)
        {
            try
            {
                if (rtbxInput.Text.Trim() == "")
                {
                    MessageBox.Show("请输入!", "提醒");
                    return;
                }
                read_code_to_disassemle();              //split each line into a string array
                rtbxOutput.Text = "";
                instr_addr = 0;
                for (int i = 0; i < code_to_disassemble.Length; i++)
                {
                    if (code_to_disassemble[i].Trim() == "")        //blank line
                    {
                        rtbxOutput.AppendText("\n");
                        continue;
                    }
                    string p2 = ass.disassemble(code_to_disassemble[i].Trim());     //get disassembled code
                    rtbxOutput.AppendText("  0x"+instr_addr.ToString("x3")+": "+code_to_disassemble[i]+fullfil_blank(code_to_disassemble[i].Length)+"| "+p2);
                    rtbxOutput.AppendText("\n");
                    instr_addr+=code_to_disassemble[i].Length;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FrmAssembler_Load(object sender, EventArgs e)
        {
            code_to_assemble=new string[1000];
            code_to_disassemble = new string[1000];           
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                rtbxInput.Text = "";
                rtbxOutput.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnInputOK_Click(object sender, EventArgs e)
        {
            try
            {
                rtbxInput.LoadFile(InputFilePath,RichTextBoxStreamType.PlainText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOutputOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (rtbxOutput.Text.Trim() == "")
                {
                    MessageBox.Show("输出框内容为空!", "提醒");
                    return;
                }
                rtbxOutput.SaveFile(OutputFilePath, RichTextBoxStreamType.PlainText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
