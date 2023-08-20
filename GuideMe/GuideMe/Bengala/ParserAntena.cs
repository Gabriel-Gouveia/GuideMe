using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Bengala
{
    public class FrameErro : AntenaFrame
    {
        public string Message { get; private set; }
        public FrameErro()
        {
            TipoFrame = TrataFrames.ErroComando;
        }
        public static FrameErro GetTagData(string[] framesString)
        {
            int failType = Convert.ToInt32(framesString[5], 16);

            /*if (packetRx.Length > 9) // has PC+EPC field
            {
                txtOperateEpc.Text = "";
                int pcEpcLen = Convert.ToInt32(packetRx[6], 16);

                for (int i = 0; i < pcEpcLen; i++)
                {
                    txtOperateEpc.Text += packetRx[i + 7] + " ";
                }
            }
            else
            {
                txtOperateEpc.Text = "";
            }*/


            if (framesString[5] == FramesFalhas.FAIL_INVENTORY_TAG_TIMEOUT)
                return new FrameErro() { Message = "FAIL_INVENTORY_TAG_TIMEOUT" };

            else if (framesString[5] == FramesFalhas.FAIL_FHSS_FAIL)
                return new FrameErro() { Message = "FAIL_FHSS_FAIL" };

            else if (framesString[5] == FramesFalhas.FAIL_ACCESS_PWD_ERROR)
                return new FrameErro() { Message = "FAIL_ACCESS_PWD_ERROR" };

            else if (framesString[5] == FramesFalhas.FAIL_READ_MEMORY_NO_TAG)
                return new FrameErro() { Message = "FAIL_READ_MEMORY_NO_TAG" };

            else if (framesString[5].Substring(0, 1) == FramesFalhas.FAIL_READ_ERROR_CODE_BASE.Substring(0, 1))
                return new FrameErro() { Message = $"FAIL_READ_ERROR_CODE_BASE Error Code: {ParseErrCode(failType)}" };

            else if (framesString[5] == FramesFalhas.FAIL_WRITE_MEMORY_NO_TAG)
                return new FrameErro() { Message = "FAIL_WRITE_MEMORY_NO_TAG" };

            else if (framesString[5].Substring(0, 1) == FramesFalhas.FAIL_WRITE_ERROR_CODE_BASE.Substring(0, 1))
                return new FrameErro() { Message = $"FAIL_WRITE_ERROR_CODE_BASE Error Code: {ParseErrCode(failType)}" };


            else if (framesString[5] == FramesFalhas.FAIL_LOCK_NO_TAG)
                return new FrameErro() { Message = "FAIL_LOCK_NO_TAG" };

            else if (framesString[5].Substring(0, 1) == FramesFalhas.FAIL_LOCK_ERROR_CODE_BASE.Substring(0, 1))
                return new FrameErro() { Message = $"FAIL_LOCK_ERROR_CODE_BASE Error Code: {ParseErrCode(failType)}" };

            else if (framesString[5] == FramesFalhas.FAIL_KILL_NO_TAG)
                return new FrameErro() { Message = "FAIL_KILL_NO_TAG" };

            else if (framesString[5].Substring(0, 1) == FramesFalhas.FAIL_KILL_ERROR_CODE_BASE.Substring(0, 1))
                return new FrameErro() { Message = $"FAIL_KILL_ERROR_CODE_BASE Error Code: {ParseErrCode(failType)}" };

            else if (framesString[5] == FramesFalhas.FAIL_NXP_CHANGE_CONFIG_NO_TAG)
                return new FrameErro() { Message = "FAIL_NXP_CHANGE_CONFIG_NO_TAG" };

            else if (framesString[5] == FramesFalhas.FAIL_NXP_CHANGE_EAS_NO_TAG)
                return new FrameErro() { Message = "FAIL_NXP_CHANGE_EAS_NO_TAG" };

            else if (framesString[5] == FramesFalhas.FAIL_NXP_CHANGE_EAS_NOT_SECURE)
                return new FrameErro() { Message = "FAIL_NXP_CHANGE_EAS_NOT_SECURE" };

            else if (framesString[5] == FramesFalhas.FAIL_NXP_EAS_ALARM_NO_TAG)
                return new FrameErro() { Message = "FAIL_NXP_EAS_ALARM_NO_TAG" };

            else if (framesString[5] == FramesFalhas.FAIL_NXP_READPROTECT_NO_TAG)
                return new FrameErro() { Message = "FAIL_NXP_READPROTECT_NO_TAG" };

            else if (framesString[5] == FramesFalhas.FAIL_NXP_RESET_READPROTECT_NO_TAG)
                return new FrameErro() { Message = "FAIL_NXP_RESET_READPROTECT_NO_TAG" };

            else if (framesString[5] == "2E") // QT Read/Write Failed
                return new FrameErro() { Message = "QT Command failed" };

            else if (framesString[5].Substring(0, 1) == FramesFalhas.FAIL_CUSTOM_CMD_BASE.Substring(0, 1))
                return new FrameErro() { Message = $"Command Executed Failed. Error Code: {ParseErrCode(failType)}" };

            else if (framesString[5] == FramesFalhas.FAIL_INVALID_PARA)
                return new FrameErro() { Message = "FAIL_INVALID_PARAMETERS" };

            else if (framesString[5] == FramesFalhas.FAIL_INVALID_CMD)
                return new FrameErro() { Message = "FAIL_INVALID_CMD" };

            return null;
        }

        private static string ParseErrCode(int errorCode)
        {
            switch (errorCode & 0x0F)
            {
                case FramesFalhas.ERROR_CODE_OTHER_ERROR:
                    return "Other Error";
                case FramesFalhas.ERROR_CODE_MEM_OVERRUN:
                    return "Memory Overrun";
                case FramesFalhas.ERROR_CODE_MEM_LOCKED:
                    return "Memory Locked";
                case FramesFalhas.ERROR_CODE_INSUFFICIENT_POWER:
                    return "Insufficient Power";
                case FramesFalhas.ERROR_CODE_NON_SPEC_ERROR:
                    return "Non-specific Error";
                default:
                    return "Non-specific Error";
            }
        }
    }
    public class FrameLeituraTag : AntenaFrame, ICloneable
    {
        public string IDMensagem { get; set; } 
        public int RSSI { get; set; }
        public string TagID { get; set; }
        public string PC { get; set; }
        public string CRC { get; set; }
        public FrameLeituraTag(int _rssi, string _tagId, string _pc, string crc)
        {
            TipoFrame = TrataFrames.LeituraTag;
            RSSI = _rssi;
            TagID = _tagId;
            PC = _pc;
            CRC = crc;
        }

        private FrameLeituraTag(FrameLeituraTag frame)
        {
            TipoFrame = TrataFrames.LeituraTag;
            RSSI = frame.RSSI;
            TagID = frame.TagID;
            PC = frame.PC;
            CRC = frame.CRC;
            IDMensagem = frame.IDMensagem;
        }

        public static FrameLeituraTag GetTagData(string[] framesString)
        {
            int rssidBm = CalculateRSSI(framesString);
            int PCEPCLength = CalculatePCEPCLength(framesString);
            string pc = $"{framesString[6]} {framesString[7]}";
            string epc = GetEPCFromFrame(framesString, PCEPCLength);
            string crc = framesString[6 + PCEPCLength] + " " + framesString[7 + PCEPCLength];
            return new FrameLeituraTag(rssidBm, epc, pc, crc);
        }
        private static int CalculateRSSI(string[] bytes)
        {
            int rssidBm = 0;
            if (bytes.Length > 5)
            {
                rssidBm = int.Parse(bytes[5], System.Globalization.NumberStyles.HexNumber);
                if (rssidBm > 127)
                {
                    rssidBm = -(-rssidBm & 0xFF);
                }
                rssidBm -= 3;
                rssidBm -= -20;

            }
            return rssidBm;
        }
        private static int CalculatePCEPCLength(string[] bytes)
        {
            int PCEPCLength = 0;
            if (bytes.Length > 6)
            {
                PCEPCLength = (int.Parse(bytes[6], System.Globalization.NumberStyles.HexNumber) / 8 + 1) * 2;
            }
            return PCEPCLength;
        }
        private static string GetEPCFromFrame(string[] bytes, int PCEPCLength)
        {
            string epc = string.Empty;
            for (int i = 0; i < PCEPCLength - 2; i++)
            {
                epc = epc + bytes[8 + i] + " ";
            }
            return epc;
        }

        public object Clone()
        {
            return new FrameLeituraTag(this);
        }
    }
    public abstract class AntenaFrame
    {
        public TrataFrames TipoFrame { get; set; }
    }
    public enum TrataFrames
    {
        LeituraTag,
        ErroComando
    }
    public static class FramesTypes
    {
        public const string comando = "00";
        public const string resposta = "01";
        public const string informacao = "02";


    }
    public static class FramesComandos
    {

        //
        // Summary:
        //     Command Type : Get Module Information
        public const string CMD_GET_MODULE_INFO = "03";

        //
        // Summary:
        //     Command Type : Set Query Parameters
        public const string CMD_SET_QUERY = "0E";

        //
        // Summary:
        //     Command Type : Get Query Parameters
        public const string CMD_GET_QUERY = "0D";

        //
        // Summary:
        //     Command Type : Read Single Tag ID(PC + EPC)
        public const string CMD_INVENTORY = "22";

        //
        // Summary:
        //     Command Type : Read Multiply Tag IDs(PC + EPC)
        public const string CMD_READ_MULTI = "27";

        //
        // Summary:
        //     Command Type : Stop Read Multiply Tag IDs(PC + EPC)
        public const string CMD_STOP_MULTI = "28";

        //
        // Summary:
        //     Command Type : Read Tag Data
        public const string CMD_READ_DATA = "39";

        //
        // Summary:
        //     Command Type : Write Tag Data
        public const string CMD_WRITE_DATA = "49";

        //
        // Summary:
        //     Command Type : Lock/Unlock Tag Memory
        public const string CMD_LOCK_UNLOCK = "82";

        //
        // Summary:
        //     Command Type : Kill Tag
        public const string CMD_KILL = "65";

        //
        // Summary:
        //     Command Type : Set Reader RF Region
        public const string CMD_SET_REGION = "07";

        //
        // Summary:
        //     Command Type : Set Reader RF Channel
        public const string CMD_SET_RF_CHANNEL = "AB";

        //
        // Summary:
        //     Command Type : Get Reader RF Channel No.
        public const string CMD_GET_RF_CHANNEL = "AA";

        //
        // Summary:
        //     Command Type : Set Reader Power Level
        public const string CMD_SET_POWER = "B6";

        //
        // Summary:
        //     Command Type : Get Reader Power Level
        public const string CMD_GET_POWER = "B7";

        //
        // Summary:
        //     Command Type : Set Reader FHSS On/Off
        public const string CMD_SET_FHSS = "AD";

        //
        // Summary:
        //     Command Type : Set Reader CW On/Off
        public const string CMD_SET_CW = "B0";

        //
        // Summary:
        //     Command Type : Set Modem Parameter
        public const string CMD_SET_MODEM_PARA = "F0";

        //
        // Summary:
        //     Command Type : Read Modem Parameter
        public const string CMD_READ_MODEM_PARA = "F1";

        //
        // Summary:
        //     Command Type : Set ISO18000-6C Select Command Parameters
        public const string CMD_SET_SELECT_PARA = "0C";

        //
        // Summary:
        //     Command Type : Get Select Command Parameters
        public const string CMD_GET_SELECT_PARA = "0B";

        //
        // Summary:
        //     Command Type : Set Inventory Mode (MODE0, Send Select Command Before Each Tag
        //     Command) (MODE1, DoNot Send Select Command Before Each Tag Command) (MODE2, Send
        //     Select Command Before Tag Commands(Read, Write, Lock, Kill) Except Inventory
        public const string CMD_SET_INVENTORY_MODE = "12";

        //
        // Summary:
        //     Command Type : Scan Jammer
        public const string CMD_SCAN_JAMMER = "F2";

        //
        // Summary:
        //     Command Type : Scan RSSI
        public const string CMD_SCAN_RSSI = "F3";

        //
        // Summary:
        //     Command Type : Control IO
        public const string CMD_IO_CONTROL = "1A";

        //
        // Summary:
        //     Command Type : Restart Reader
        public const string CMD_RESTART = "19";

        //
        // Summary:
        //     Command Type : Set Reader Mode(Dense Reader Mode or High-sensitivity Mode)
        public const string CMD_SET_READER_ENV_MODE = "F5";

        //
        // Summary:
        //     Command Type : Insert RF Channel to the FHSS Frequency Look-up Table
        public const string CMD_INSERT_FHSS_CHANNEL = "A9";

        //
        // Summary:
        //     Command Type : Set Reader to Sleep Mode
        public const string CMD_SLEEP_MODE = "17";

        //
        // Summary:
        //     Command Type : Set Reader Idle Time, after These Minutes, the Reader Will Go
        //     to Sleep Mode
        public const string CMD_SET_SLEEP_TIME = "1D";

        //
        // Summary:
        //     Command Type : Load Configuration From Non-volatile Memory
        public const string CMD_LOAD_NV_CONFIG = "0A";

        //
        // Summary:
        //     Command Type : Save Configuration to Non-volatile Memory
        public const string CMD_SAVE_NV_CONFIG = "09";

        //
        // Summary:
        //     Command Type : Change Config Command for NXP G2X Tags
        public const string CMD_NXP_CHANGE_CONFIG = "E0";

        //
        // Summary:
        //     Command Type : ReadProtect Command for NXP G2X Tags Reset ReadProtect Command
        //     uses the same command code but with different parameter
        public const string CMD_NXP_READPROTECT = "E1";

        //
        // Summary:
        //     Command Type : Reset ReadProtect Command for NXP G2X Tags
        public const string CMD_NXP_RESET_READPROTECT = "E2";

        //
        // Summary:
        //     Command Type : ChangeEAS Command for NXP G2X Tags
        public const string CMD_NXP_CHANGE_EAS = "E3";

        //
        // Summary:
        //     Command Type : EAS_Alarm Command for NXP G2X Tags
        public const string CMD_NXP_EAS_ALARM = "E4";

        //
        // Summary:
        //     Command Type : QT Read Command for Monza Tags
        public const string CMD_IPJ_MONZA_QT_READ = "E5";

        //
        // Summary:
        //     Command Type : QT Write Command for Monza Tags
        public const string CMD_IPJ_MONZA_QT_WRITE = "E6";

        //
        // Summary:
        //     Command Execute Fail Type
        public const string CMD_EXE_FAILED = "FF";

    }
    public static class FramesFalhas
    {
        //
        // Summary:
        //     Fail Type : Command Parameter Invalid
        public const string FAIL_INVALID_PARA = "0E";

        //
        // Summary:
        //     Fail Type : Read Tag ID Time out
        public const string FAIL_INVENTORY_TAG_TIMEOUT = "15";

        //
        // Summary:
        //     Fail Type : Invalid Command
        public const string FAIL_INVALID_CMD = "17";

        //
        // Summary:
        //     Fail Type : FHSS Failed
        public const string FAIL_FHSS_FAIL = "20";

        //
        // Summary:
        //     Fail Type : Access Password Error
        public const string FAIL_ACCESS_PWD_ERROR = "16";

        //
        // Summary:
        //     Fail Type : Read Tag Memory No Tag Response
        public const string FAIL_READ_MEMORY_NO_TAG = "09";

        //
        // Summary:
        //     Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Read Operation.
        //     The Error Code Will Be Added to this Code.
        public const string FAIL_READ_ERROR_CODE_BASE = "A0";

        //
        // Summary:
        //     Fail Type : Write Tag Memory No Tag Response
        public const string FAIL_WRITE_MEMORY_NO_TAG = "10";

        //
        // Summary:
        //     Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Write Operation.
        //     The Error Code Will Be Added to this Code.
        public const string FAIL_WRITE_ERROR_CODE_BASE = "B0";

        //
        // Summary:
        //     Fail Type : Lock Tag No Tag Response
        public const string FAIL_LOCK_NO_TAG = "13";

        //
        // Summary:
        //     Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Lock Operation.
        //     The Error Code Will Be Added to this Code.
        public const string FAIL_LOCK_ERROR_CODE_BASE = "C0";

        //
        // Summary:
        //     Fail Type : Kill Tag No Tag Response
        public const string FAIL_KILL_NO_TAG = "12";

        //
        // Summary:
        //     Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Kill Operation.
        //     The Error Code Will Be Added to this Code.
        public const string FAIL_KILL_ERROR_CODE_BASE = "D0";

        //
        // Summary:
        //     Fail Type : NXP Change Config Command No Tag Response
        public const string FAIL_NXP_CHANGE_CONFIG_NO_TAG = "1A";

        //
        // Summary:
        //     Fail Type : NXP ReadProtect Command No Tag Response
        public const string FAIL_NXP_READPROTECT_NO_TAG = "2A";

        //
        // Summary:
        //     Fail Type : NXP Reset ReadProtect Command No Tag Response
        public const string FAIL_NXP_RESET_READPROTECT_NO_TAG = "2B";

        //
        // Summary:
        //     Fail Type : NXP Change EAS Command No Tag Response
        public const string FAIL_NXP_CHANGE_EAS_NO_TAG = "1B";

        //
        // Summary:
        //     Fail Type : When Executing NXP Change Config Command , Tag is Not in Secure State.
        //     NXP tag responses to Change EAS command only in secure state, so the access password
        //     must not be 0.
        public const string FAIL_NXP_CHANGE_EAS_NOT_SECURE = "1C";

        //
        // Summary:
        //     Fail Type : NXP EAS Alarm Command No Tag Response
        public const string FAIL_NXP_EAS_ALARM_NO_TAG = "1D";

        //
        // Summary:
        //     Fail Type : QT Read/Write Command No Tag Response
        public const string FAIL_IPJ_MONZA_QT_NO_TAG = "2E";

        //
        // Summary:
        //     Fail Type : Error Code Caused By Custom(NXP tags, Monza QT tags or other tags)
        //     Operation. The Error Code Will Be Added to this Code.
        public const string FAIL_CUSTOM_CMD_BASE = "E0";

        //
        // Summary:
        //     Error Code(according to C1Gen2 Protocol) : Other Error
        public const int ERROR_CODE_OTHER_ERROR = 0;

        //
        // Summary:
        //     Error Code(according to C1Gen2 Protocol) : Memory Overrun
        public const int ERROR_CODE_MEM_OVERRUN = 3;

        //
        // Summary:
        //     Error Code(according to C1Gen2 Protocol) : Memory Locked
        public const int ERROR_CODE_MEM_LOCKED = 4;

        //
        // Summary:
        //     Error Code(according to C1Gen2 Protocol) : Insufficient Power
        public const int ERROR_CODE_INSUFFICIENT_POWER = 11;

        //
        // Summary:
        //     Error Code(according to C1Gen2 Protocol) : Non-specific Error
        public const int ERROR_CODE_NON_SPEC_ERROR = 15;
    }

    public static class ParserAntena
    {
        private const char caracterSeparador = ' ';
        private const string inicioComunicacao = "bb";
        private const string fimComunicacao = "7e";

        private static string[] GetBytesAsString(string frame, char separador = caracterSeparador)
        {
            string[] retorno = null;
            try
            {
                retorno = frame.Split(separador);
                if (retorno.Length <= 1)
                    retorno = null;

            }
            catch (Exception erro)
            {
            }
            return retorno;
        }
        private static TrataFrames ParseHeader(string[] frame, ref bool erro)
        {
            erro = false;
            bool frameInfoRecebido = false, frameRespostaRecebida = false, leituraEtiqueta = false, erroNoComando = false;
            for (int n = 0; n < frame.Length; n++)
            {
                if (n == 0 && frame[n].ToUpper() != inicioComunicacao.ToUpper())
                {
                    erro = true;
                    break;
                }
                else if (n == 1)
                {
                    if (frame[n].ToUpper() == FramesTypes.informacao.ToUpper())
                        frameInfoRecebido = true;

                    else if (frame[n].ToUpper() == FramesTypes.resposta.ToUpper())
                        frameRespostaRecebida = true;
                }
                else if (n == 2)
                {
                    if (frameInfoRecebido && frame[n].ToUpper() == FramesComandos.CMD_INVENTORY.ToUpper())
                    {
                        leituraEtiqueta = true;
                        break;
                    }
                    else if (frameRespostaRecebida && frame[n].ToUpper() == FramesComandos.CMD_EXE_FAILED.ToUpper())
                    {
                        erroNoComando = true;
                        break;
                    }
                }
            }
            if (frame[frame.Length - 1].ToUpper() != fimComunicacao.ToUpper())
                erro = true;

            if (leituraEtiqueta)
                return TrataFrames.LeituraTag;
            else if (erroNoComando)
                return TrataFrames.ErroComando;


            return TrataFrames.LeituraTag;
        }

        public static AntenaFrame ParseData(string frame)
        {
            bool erro = false;
            string[] framesString = GetBytesAsString(frame);
            if (framesString != null && framesString.Length > 1)
            {
                TrataFrames trataFrameRecebido = ParseHeader(framesString, ref erro);
                if (!erro)
                {
                    switch (trataFrameRecebido)
                    {
                        case TrataFrames.LeituraTag:
                            return FrameLeituraTag.GetTagData(framesString);

                        case TrataFrames.ErroComando:
                            return FrameErro.GetTagData(framesString);

                    }
                }

            }
            return null;
        }


    }
}
