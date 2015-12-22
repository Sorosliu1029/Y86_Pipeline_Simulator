using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.IO;

/******************************************************     
 * FileName:  Pipe.cs
 * Writer:  Liu Yang
 * create Date: 2015-6-5
 * Main Content: 
 *  A pipeline simulator class. 
 *  Most of this part is a translation of the "waside-hcl.pdf" document, from HCL language to C# language.
 *  The basic idea is that we can record each cycle's information of the Pipeline into .txt documents, because once
 *  the Y86 instructions have been executed, each cycle's status can be ensured. And we can record these information 
 *  for the UI presetation and update.
 *******************************************************/

namespace Y86_Pipeline_Simulator
{
    class Pipe
    {
        const int MAX_MEM_CAP = (1<<16);          //the allocated memory capacity
        public byte[] mem;                                  //memory for instructions and data
        int[] reg_file;                                           //register file
        public int cycle_cnt;
        //the pipeline variable. 
        //Upper first letter is the pipeline registers' variable, e.g. "D_stat"
        //Lower first letter is the pipeline stages' variable, e.g. "d_valA"
        int F_predPC;
        int f_stat, f_icode, f_ifun, f_valC, f_valP, f_pc, f_predPC;
        byte f_rA, f_rB;
        bool imem_error, instr_valid;
        int D_stat, D_icode, D_ifun, D_rA, D_rB, D_valC, D_valP;
        int d_stat, d_icode, d_ifun, d_valC, d_valA, d_valB, d_dstE, d_dstM, d_srcA, d_srcB;
        int E_stat, E_icode, E_ifun, E_valC, E_valA, E_valB, E_dstE, E_dstM, E_srcA, E_srcB;
        int e_stat, e_icode, e_valE, e_valA, e_dstE, e_dstM;
        bool e_Cnd;
        int M_stat, M_icode, M_valE, M_valA, M_dstE, M_dstM;
        bool M_Cnd;
        int m_stat, m_icode, m_valE, m_valM, m_dstE, m_dstM;
        bool dmem_error;
        int W_stat, W_icode, W_valE, W_valM, W_dstE, W_dstM;
        int Stat;
        byte imem_icode, imem_ifun;              
        //Pipeline register control signals
        bool F_stall, F_bubble, D_stall, D_bubble, E_stall, E_bubble,M_stall, M_bubble,W_stall, W_bubble;
        string Forwarding_A, Forwarding_B;
        bool ZF, SF, OF;                        //condition code
        int aluA, aluB;
        int alufun;
        bool set_cc;
        int mem_addr;
        bool mem_read, mem_write;
        public string writeDirectory;   //directory for writing the record documents
        int ini_stack;                          //record the allocated stack space
       
        public Pipe()
        {
            ini_Pipe();
        }   
        public void ini_Pipe()      //initialise pipeline
        {
            cycle_cnt = F_predPC = f_icode = f_ifun = f_valC = f_valP = f_pc = f_predPC =
              D_icode = D_ifun = D_valC = D_valP = d_icode = d_ifun =
             d_valC = d_valA = d_valB  = E_icode = E_ifun =
             E_valC = E_valA = E_valB  = e_icode =
             e_valE  = M_icode = M_valE = M_valA =
              m_icode = m_valE = m_valM = W_icode = W_valE = W_valM =
             aluA = aluB = alufun = mem_addr = 0;
            D_rA = D_rB = d_dstE = d_dstM = d_srcA = d_srcB = E_dstE = E_dstM = E_srcA =
            E_srcB = e_dstE = e_dstM = M_dstE = M_dstM = m_dstE = m_dstM = W_dstE = W_dstM = 8;
            imem_icode = imem_ifun = 0;
            f_stat = D_stat = d_stat = E_stat = e_stat = M_stat = m_stat = W_stat = Stat = 1;
            f_rA = f_rB = 0;
            imem_error = instr_valid = dmem_error = ZF = SF = OF = e_Cnd = M_Cnd = set_cc = mem_read = mem_write = false;
            F_stall = F_bubble = D_stall = D_bubble = E_stall = E_bubble = M_bubble = M_stall = W_bubble = W_stall = false;
            Forwarding_A = Forwarding_B = "N";
            mem = new byte[MAX_MEM_CAP];
            for (int i = 0; i < MAX_MEM_CAP; i++) mem[i] = 0;
            reg_file = new int[9];
            for (int i = 0; i < 9; i++) reg_file[i] = 0;
        }

        /*combine two byte variables into one byte variable 
         * mainly used for combining assemble code into an 8-bits one
         * and put into the memory
         */
        public byte combine(byte a, byte b) 
        {
            return (byte)((b & (0xf)) | (a << 4));
        }

        /*decompose one byte variavle into two byte variable
         * mainly used for decomposing instructions into icode, ifun, rA, rB
         */
        public void decompose(ref byte a, ref byte b, byte ab)
        {
            a = (byte)((ab >> 4) & (0xf));
            b = (byte)(ab & (0xf));
        }

        /*get 4 bytes value from the memory, according to the start address
         * mainly used for getting value for valA, valB, valC, valP, etc.
         */
        public int get_4_bytes_val(int start_addr)
        {
            byte[] n =new byte[8];
            for(int i=0;i<8;i++)
                n[i]=0;
            for(int i=0;i<4;i++)
                decompose(ref n[2*i],ref n[2*i+1],mem[start_addr+3-i]);
            string result = "";
            for (int i = 0; i < 8; i++)
                result += n[i].ToString("x");
            return Convert.ToInt32(result,16);
        }

        /*write 4 bytes value to the memory, according to the start address
         * mainly used for writing memory in the memory stage
         */
        public void write_4_bytes_val(int value, int start_addr)
        {
            string four_bytes=value.ToString("x8");
            byte[] b = new byte[8];
            for (int i = 0; i < 8; i++)
                b[i] = Convert.ToByte(four_bytes[i].ToString(),16);
            for (int i = 0; i < 4; i++)
            {
                mem[start_addr + 3 - i] = combine(b[2 * i], b[2 * i + 1]);
            }
        }

        /*before the start of the pipeline
         * mainly used for loading instructions and data into the memory
         */
        void prepareMemory(string readPath)
        {
            StreamReader sr = new StreamReader(readPath, Encoding.Default);
            string line;            
            int j;
            int index;
            while ((line = sr.ReadLine()) != null)
            {
                j = 9;
                if (line.Substring(2, 2) == "0x")
                {
                    index = Convert.ToInt32(line.Substring(4, 3), 16);      //start index for instruction or data in the memory
                    while (line[j] != ' ')
                    {
                        mem[index++] = combine(Convert.ToByte(line[j].ToString(),16), Convert.ToByte(line[j+1].ToString(),16));
                        j += 2;
                    }
                }                
            }
            sr.Close();
        }

        //fetch stage         
        void fetch()
        {
            //Select PC module
            if (M_icode == 7 && !M_Cnd)
                f_pc = M_valA;
            else if (W_icode == 9)
                f_pc = W_valM;
            else
                f_pc = F_predPC;
            //imem_error module & f_icode, f_ifun module
            if (f_pc > MAX_MEM_CAP)
                imem_error = true;
            else
            {
                imem_error = false;
                decompose(ref imem_icode, ref imem_ifun, mem[f_pc]);        //get icode and ifun from the memory where f_pc points to
            }                
            f_icode = imem_error ? 0 : imem_icode; 
            f_ifun = imem_error ? 0 : imem_ifun;
            //instr_valid module
            if (f_icode >= 0 && f_icode <= 11)
            {
                instr_valid = true;
                if (f_icode == 2||f_icode==7)
                {
                    if (!(f_ifun >= 0 && f_ifun <= 6))
                        instr_valid = false;
                }
                if(f_icode==6)
                {
                    if (!(f_ifun >= 0 && f_ifun <= 3))
                        instr_valid = false;
                }
            }
            else instr_valid = false;
            //f_stat module
            if (imem_error) 
                f_stat = 3;
            else if (!instr_valid)
                f_stat = 4;
            else if (f_icode == 1)
                f_stat = 2;
            else f_stat = 1;
            //PC increment module & fetch module
            if (f_stat == 1)
            {
                if ((f_icode >= 2 && f_icode <= 6) || f_icode == 10 || f_icode == 11)
                {
                    decompose(ref f_rA, ref f_rB, mem[f_pc + 1]);
                    f_valP = f_pc + 2;
                }
                if (f_icode >= 3 && f_icode <= 5)
                {
                    f_valC = get_4_bytes_val(f_pc + 2);
                    f_valP = f_pc + 6;
                }
                if (f_icode == 7 || f_icode == 8)
                {
                    f_valC = get_4_bytes_val(f_pc + 1);
                    f_valP = f_pc + 5;
                }
                if (f_icode == 0 || f_icode == 1 || f_icode == 9)
                    f_valP = f_pc + 1;
            }
            //Predict PC module
            if (f_icode == 7 || f_icode == 8)
                f_predPC = f_valC;
            else f_predPC = f_valP;
        }

        //decode stage
        void decode()
        {
            d_stat = D_stat; d_icode = D_icode; d_ifun = D_ifun; d_valC = D_valC;
            //srcA module
            if (D_icode == 2 || D_icode == 4 || D_icode == 6 || D_icode == 10)
                d_srcA = D_rA;
            else if (D_icode == 11 || D_icode == 9)
                d_srcA = 4;
            else d_srcA = 8;
            //srcB module
            if (D_icode >= 4 && D_icode <= 6)
                d_srcB = D_rB;
            else if (D_icode >= 8 && D_icode <= 11)
                d_srcB = 4;
            else d_srcB = 8;
            //dstE module
            if (D_icode == 2 || D_icode == 3 || D_icode == 6)
                d_dstE = D_rB;
            else if (D_icode >= 8 && D_icode <= 11)
                d_dstE = 4;
            else d_dstE = 8;
            //dstM module
            if (D_icode == 5 || D_icode == 11)
                d_dstM = D_rA;
            else d_dstM = 8;
            //Sel+Fwd A module
            if (D_icode == 8 || D_icode == 7)
            {
                d_valA = D_valP;
                Forwarding_A = "NULL";
            }
            else if (d_srcA == e_dstE)
            {
                d_valA = e_valE;
                Forwarding_A = "e_valE:0x" + e_valE.ToString("x8");
            }
            else if (d_srcA == M_dstM)
            {
                d_valA = m_valM;
                Forwarding_A = "m_valM:0x" + m_valM.ToString("x8");
            }
            else if (d_srcA == M_dstE)
            {
                d_valA = M_valE;
                Forwarding_A = "M_valE:0x" + M_valE.ToString("x8");
            }
            else if (d_srcA == W_dstM)
            {
                d_valA = W_valM;
                Forwarding_A = "W_valM:0x" + W_valM.ToString("x8");
            }
            else if (d_srcA == W_dstE)
            {
                d_valA = W_valE;
                Forwarding_A = "W_valE:0x" + W_valE.ToString("x8");
            }
            else
            {
                d_valA = reg_file[d_srcA];
                Forwarding_A = "NULL";
            }
            //Fwd B module
            if (d_srcB == e_dstE)
            {
                d_valB = e_valE;
                Forwarding_B = "e_valE:0x" + e_valE.ToString("x8");
            }
            else if (d_srcB == M_dstM)
            {
                d_valB = m_valM;
                Forwarding_B = "m_valM:0x" + m_valM.ToString("x8");
            }
            else if (d_srcB == M_dstE)
            {
                d_valB = M_valE;
                Forwarding_B = "M_valE:0x" + M_valE.ToString("x8");
            }
            else if (d_srcB == W_dstM)
            {
                d_valB = W_valM;
                Forwarding_B = "W_valM:0x" + W_valM.ToString("x8");
            }
            else if (d_srcB == W_dstE)
            {
                d_valB = W_valE;
                Forwarding_B = "W_valE:0x" + W_valE.ToString("x8");
            }
            else
            {
                d_valB = reg_file[d_srcB];
                Forwarding_B = "NULL";
            }
        }

        //execute stage
        void execute()
        {
            e_stat = E_stat; e_icode = E_icode; e_dstM = E_dstM; e_valA = E_valA;
            //ALU A module
            if (E_icode == 2 || E_icode == 6)
                aluA = E_valA;
            else if (E_icode >= 3 && E_icode <= 5)
                aluA = E_valC;
            else if (E_icode == 8 || E_icode == 10)
                aluA = -4;
            else if (E_icode == 9 || E_icode == 11)
                aluA = 4;
            else aluA = 0;
            //ALU B module
            if (E_icode == 4 || E_icode == 5 || E_icode == 6 || E_icode == 8 || E_icode == 10 || E_icode == 9 || E_icode == 11)
                aluB = E_valB;
            else if (E_icode == 2 || E_icode == 3)
                aluB = 0;
            else aluB = 0;
            //ALU fun. module
            if (E_icode == 6)
                alufun = E_ifun;
            else alufun = 0;
            //ALU module
            switch (alufun)
            {
                case 0 : e_valE = aluB+ aluA; break;
                case 1: e_valE = aluB - aluA; break;
                case 2: e_valE = aluB & aluA; break;
                case 3: e_valE = aluB ^ aluA; break;
                default: e_valE = 0; break;
            }
            //CC module
            if (E_icode == 6 && m_stat != 3 && m_stat != 4 && m_stat != 2 && W_stat != 3 && W_stat != 4 && W_stat != 2)
                set_cc = true;
            else set_cc = false;
            if (set_cc)
            {
                if (e_valE < 0)
                    SF = true;
                else SF = false;
                if (e_valE == 0)
                    ZF = true;
                else ZF = false;
                if ((aluA < 0 == aluB < 0) && (aluA < 0 != e_valE < 0))
                    OF = true;
                else OF = false;
            }
            //e_Cnd module            
            if (E_icode == 7 || E_icode == 2)
            {
                switch (E_ifun)
                {
                    case 0: e_Cnd = true; break;                                                                //jmp or rrmovl
                    case 1: if (ZF || SF) e_Cnd = true; else e_Cnd = false; break;               //jle or cmovle
                    case 2: if (SF) e_Cnd = true; else e_Cnd = false; break;                       //jl or cmovl
                    case 3: if (ZF) e_Cnd = true; else e_Cnd = false; break;                       //je or comve
                    case 4: if (!ZF) e_Cnd = true; else e_Cnd = false; break;                      //jne or cmovne
                    case 5: if (!SF) e_Cnd = true; else e_Cnd = false; break;                      //jge or comvge
                    case 6: if (!ZF && !SF) e_Cnd = true; else e_Cnd = false; break;         //jg or comvg
                }
            }
            else e_Cnd = false;
            //dstE module
            if (E_icode == 2 && !e_Cnd)
                e_dstE = 8;
            else e_dstE = E_dstE;
        }

        //memory stage
        void memory()
        {
            m_icode = M_icode; m_valE = M_valE; m_dstE = M_dstE; m_dstM = M_dstM;
            //Addr module
            if (M_icode == 4 || M_icode == 10 || M_icode == 8 || M_icode == 5)
                mem_addr = M_valE;
            else if (M_icode == 9 || M_icode == 11)
                mem_addr = M_valA;
            //Memory control module
            if (M_icode == 5 || M_icode == 11 || M_icode == 9)
                mem_read = true;
            else mem_read = false;
            if (M_icode == 4 || M_icode == 10 || M_icode == 8)
                mem_write = true;
            else mem_write = false;
            //read / write memory
            if (mem_addr > MAX_MEM_CAP)
                dmem_error = true;
            else
            {
                dmem_error = false;
                if (mem_read)
                {
                    m_valM = get_4_bytes_val(mem_addr);
                }
                else m_valM = 0;
                if (mem_write)
                {
                    write_4_bytes_val(M_valA, mem_addr);
                }
            }
            m_stat = dmem_error ? 3 : M_stat;        //3 is for ADR
        }

        //write back stage
        void write_back()
        {
            if (W_stat == 5)     // assume bubble is 5
                Stat = 1;
            else Stat = W_stat;
            if (Stat == 1)      //only if stat is AOK can we update register file
            {
                if (W_dstE != 8)
                    reg_file[W_dstE] = W_valE;
                if (W_dstM != 8)
                    reg_file[W_dstM] = W_valM;
            }
        }

        //pass value from Predict PC module to Fetch register
        void val_pass2_fetch_reg()
        {
            if (!F_stall)
                F_predPC = f_predPC;
        }

        //pass value from fetch stage to Decode register
        void val_pass2_decode_reg()
        {
            if (!D_stall)
            {
                D_stat = f_stat; D_icode = f_icode; D_ifun = f_ifun; D_rA = f_rA; D_rB = f_rB; D_valC = f_valC; D_valP = f_valP;
            }
            if (D_bubble) //if stat is bubble , stat value = 5
            {
                D_stat = 5; D_icode = 0; D_ifun = 0; D_rA = 8; D_rB = 8; D_valC = 0; D_valP = 0;
            }
        }

        //pass value from decode stage to Execute register
        void val_pass2_execute_reg()
        {
            if (E_bubble)
            {
                E_stat = 5; E_icode = 0; E_ifun = 0; E_valC = 0; E_valA = 0; E_valB = 0; E_dstE = 8; E_dstM = 8; E_srcA = 8; E_srcB = 8;
            }
            else
            {
                E_stat = d_stat; E_icode = d_icode; E_ifun = d_ifun; E_valC = d_valC; E_valA = d_valA; E_valB = d_valB; E_dstE = d_dstE; E_dstM = d_dstM; E_srcA = d_srcA; E_srcB = d_srcB;
            }
        }

        //pass value from execute stage to Memory register
        void val_pass2_memory_reg()
        {
            if (M_bubble)
            {
                M_stat = 5; M_icode = 0; M_Cnd = false; M_valE = 0; M_valA = 0; M_dstE = 8; M_dstM = 8;
            }
            else
            {
                M_stat = e_stat; M_icode = e_icode; M_Cnd = e_Cnd; M_valE = e_valE; M_valA = e_valA; M_dstE = e_dstE; M_dstM = e_dstM;
            }
        }

        //pass value from memory stage to Write Back register
        void val_pass2_writeback_reg()
        {
            if (!W_stall)
            {
                W_stat = m_stat; W_icode = m_icode; W_valE = m_valE; W_valM = m_valM; W_dstE = m_dstE; W_dstM = m_dstM;
            }
        }

        /*Clock Rise
         * increase the cycle count
         * decide the pipeline register control signal
         * then pass the stage values to the pipeline registers
         */
        void clock_rise()
        {
            cycle_cnt++;
            F_bubble = false;

            if ((E_icode == 5 || E_icode == 11) && (E_dstM == d_srcA || E_dstM == d_srcB) || (D_icode == 9 || E_icode == 9 || M_icode == 9))
                F_stall = true;
            else F_stall = false;

            if ((E_icode == 5 || E_icode == 11) && (E_dstM == d_srcA || E_dstM == d_srcB))
                D_stall = true;
            else D_stall = false;

            if ((E_icode == 7 && !e_Cnd) || !((E_icode == 5 || E_icode == 11) && (E_dstM == d_srcA || E_dstM == d_srcB)) && (D_icode == 9 || E_icode == 9 || M_icode == 9))
                D_bubble = true;
            else D_bubble = false;

            E_stall = false;

            if ((E_icode == 7 && !e_Cnd) || (E_icode == 5 || E_icode == 11) && (E_dstM == d_srcA || E_dstM == d_srcB))
                E_bubble = true;
            else E_bubble = false;

            M_stall = false;

            if ((m_stat == 2 || m_stat == 3 || m_stat == 4) || (W_stat == 2 || W_stat == 3 || W_stat == 4))
                M_bubble = true;
            else M_bubble = false;

            if (W_stat == 2 || W_stat == 3 || W_stat == 4)
                W_stall = true;
            else W_stall = false;

            W_bubble = false;

            val_pass2_fetch_reg();
            val_pass2_decode_reg();
            val_pass2_execute_reg();
            val_pass2_memory_reg();
            val_pass2_writeback_reg();
        }
        void print_cycle_info(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("Cycle_" + cycle_cnt);
            sw.WriteLine("--------------------");
            sw.Close();
        }
        void print_fetch_reg(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("FETCH:");
            sw.WriteLine("\tF_predPC\t= 0x" + F_predPC.ToString("x8"));
            sw.WriteLine();
            sw.Close();
        }
        void print_decode_reg(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("DECODE:");
            sw.WriteLine("\tD_icode\t\t= 0x"+D_icode.ToString("x"));
            sw.WriteLine("\tD_ifun\t\t= 0x" + D_ifun.ToString("x"));
            sw.WriteLine("\tD_rA\t\t= 0x" + D_rA.ToString("x"));
            sw.WriteLine("\tD_rB\t\t= 0x" + D_rB.ToString("x"));
            sw.WriteLine("\tD_valC\t\t= 0x" + D_valC.ToString("x8"));
            sw.WriteLine("\tD_valP\t\t= 0x" + D_valP.ToString("x8"));
            sw.WriteLine();
            sw.Close();
        }
        void print_execute_reg(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("EXECUTE:");
            sw.WriteLine("\tE_icode\t\t= 0x" + E_icode.ToString("x"));
            sw.WriteLine("\tE_ifun\t\t= 0x" + E_ifun.ToString("x"));
            sw.WriteLine("\tE_valC\t\t= 0x" + E_valC.ToString("x8"));
            sw.WriteLine("\tE_valA\t\t= 0x" + E_valA.ToString("x8"));
            sw.WriteLine("\tE_valB\t\t= 0x" + E_valB.ToString("x8"));
            sw.WriteLine("\tE_dstE\t\t= 0x" + E_dstE.ToString("x"));
            sw.WriteLine("\tE_dstM\t\t= 0x" + E_dstM.ToString("x"));
            sw.WriteLine("\tE_srcA\t\t= 0x" + E_srcA.ToString("x"));
            sw.WriteLine("\tE_srcB\t\t= 0x" + E_srcB.ToString("x"));
            sw.WriteLine();
            sw.Close();
        }
        void print_memory_reg(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("MEMORY:");
            sw.WriteLine("\tM_icode\t\t= 0x" + M_icode.ToString("x"));
            sw.WriteLine("\tM_Bch\t\t= " + M_Cnd.ToString().ToLower());
            sw.WriteLine("\tM_valE\t\t= 0x" + M_valE.ToString("x8"));
            sw.WriteLine("\tM_valA\t\t= 0x" + M_valA.ToString("x8"));
            sw.WriteLine("\tM_dstE\t\t= 0x" + M_dstE.ToString("x"));
            sw.WriteLine("\tM_dstM\t\t= 0x" + M_dstM.ToString("x"));
            sw.WriteLine();
            sw.Close();
        }
        void print_writeback_reg(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("WRITE BACK:");
            sw.WriteLine("\tW_icode\t\t= 0x" + W_icode.ToString("x"));
            sw.WriteLine("\tW_valE\t\t= 0x" + W_valE.ToString("x8"));
            sw.WriteLine("\tW_valM\t\t= 0x" + W_valM.ToString("x8"));
            sw.WriteLine("\tW_dstE\t\t= 0x" + W_dstE.ToString("x"));
            sw.WriteLine("\tW_dstM\t\t= 0x" + W_dstM.ToString("x"));
            sw.WriteLine();
            sw.Close();
        }

        //print the output .txt document
        void print_Pipeline_regs(string writePath)
        {
            print_cycle_info(writePath);
            print_fetch_reg(writePath);
            print_decode_reg(writePath);
            print_execute_reg(writePath);
            print_memory_reg(writePath);
            print_writeback_reg(writePath);
        }

        //print pipeline registers value record, one cycle per line
        void print_Pipeline_regs_simpler(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("" + F_predPC.ToString("x8") + "," + D_stat + "," + D_icode.ToString("x") + "," + D_ifun + "," + D_rA + ","
                + D_rB + "," + D_valC.ToString("x8") + "," + D_valP.ToString("x8") + "," + E_stat + "," + E_icode.ToString("x") + "," + E_ifun +
                "," + E_valC.ToString("x8") + "," + E_valA.ToString("x8") + "," + E_valB.ToString("x8") + "," + E_dstE + "," + E_dstM +
                "," + E_srcA + "," + E_srcB + "," + M_stat + "," + M_icode.ToString("x") + "," + M_Cnd.ToString().ToLower() + "," + M_valE.ToString("x8") +
                "," + M_valA.ToString("x8") + "," + M_dstE + "," + M_dstM + "," + W_stat + "," + W_icode.ToString("x") + "," + W_valE.ToString("x8") + 
                "," + W_valM.ToString("x8") + "," + W_dstE + "," + W_dstM);
            sw.Close();
        }

        //print combine logic module value record, one cycle per line
        void print_cmb_logic_val(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("" + f_pc.ToString("x8") + "," + M_valA.ToString("x8") + "," + W_valM.ToString("x8") + "," + imem_error + 
                "," + instr_valid + "," + W_valE.ToString("x8") + "," + d_srcA + "," + d_srcB + "," +
                e_dstE + "," + e_Cnd.ToString().ToLower() + "," + M_Cnd.ToString().ToLower() + "," + M_valE.ToString("x8") + "," 
                + mem_read + "," + mem_write + "," + m_stat + "," + dmem_error + "," + m_valM.ToString("x8") + "," +Stat);
            sw.Close();
        }

        //print register files value record, one cycle per line
        void print_reg_file(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("" + reg_file[0].ToString("x8") + "," + reg_file[1].ToString("x8") + "," + reg_file[2].ToString("x8") 
                + "," + reg_file[3].ToString("x8") + "," + reg_file[4].ToString("x8") +
                "," + reg_file[5].ToString("x8") + "," + reg_file[6].ToString("x8") + "," + reg_file[7].ToString("x8"));
            sw.Close();
        }

        //1 for true, 0 for false
        int trans_cc(bool f)
        {
            return f ? 1 : 0;
        }

        //print condition code value record, one cycle per line
        void print_cc(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("" + trans_cc(ZF) + "," +trans_cc(SF) + "," + trans_cc(OF));
            sw.Close();
        }

        //print pipeline control logic value record, one cycle per line
        void print_pipeline_control(string writePath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            sw.WriteLine("" + F_stall + "," + F_bubble + "," + D_stall + "," + D_bubble + "," + E_stall + ","
                + E_bubble + "," + M_stall + "," + M_bubble + "," + W_stall + "," + W_bubble + "," +
                Forwarding_A + "," + Forwarding_B);
            sw.Close();
        }

        //print stack information, one cycle per line
        void print_stack(string writePath)
        {
            string line = "";
            StreamWriter sw = new StreamWriter(writePath, true);
            line += ini_stack + "&" + reg_file[4] + "&" + reg_file[5];          //stack bottom , %esp value , %ebp value
            for (int i = ini_stack; i >= reg_file[4]; i -= 4)
            {
                line += ("," + get_4_bytes_val(i).ToString("x8"));
            }
            sw.WriteLine(line);
            sw.Close();
        }

        //print Y86 instructions, for the UI persentation
        void print_Y86_instr(string writePath, string readPath)
        {
            StreamWriter sw = new StreamWriter(writePath, true);
            StreamReader sr = new StreamReader(readPath, Encoding.Default);
            string line;
            int j;
            while ((line = sr.ReadLine()) != null)
            {
                j = line.IndexOf('|');
                sw.WriteLine(line.Substring(j + 2));
            }
            sr.Close();
            sw.Close();
        }

        //start a new stage in a cycle
        void start_new_stage()
        {
            write_back();
            memory();
            execute();
            decode();
            fetch();
        }

        //auto complete all instructions through the pipeline
        public void complete_pipeline(string readPath, string writePath)
        {
            if (File.Exists(writePath))
            {
                File.Delete(writePath);
            }
            prepareMemory(readPath);
            ini_stack = get_4_bytes_val(2);
            print_Y86_instr(writeDirectory + "\\" + "Y86_instr.txt", readPath);
            do
            {
                print_Pipeline_regs(writePath);
                print_Pipeline_regs_simpler(writeDirectory + "\\" + "pipeline_regs_record.txt");
                print_reg_file(writeDirectory + "\\" + "reg_file_record.txt");
                print_cc(writeDirectory + "\\" + "cc_record.txt");
                print_cmb_logic_val(writeDirectory + "\\" + "cmb_logic_val_record.txt");
                print_pipeline_control(writeDirectory + "\\" + "pipeline_control_record.txt");
                print_stack(writeDirectory + "\\" + "stack_record.txt");
                start_new_stage();
                clock_rise();
            } while (Stat == 1);                     //until the pipeline status is HLT or ADR or INS   
        }
    }
}
