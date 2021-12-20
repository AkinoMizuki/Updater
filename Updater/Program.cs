using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;


namespace Updater
{/*=== コンソールアプリケーション ===*/

    class ini_Settings
    {/*=== 設定用変数 ===*/

        /*********************************************************************
        *   定義
        *********************************************************************/

        /*=== 設定ファイル ===*/
        public static string ConfFile = "Updater.config";
        /*=== ログファイル名 ===*/
        public string LogFile = "UpdateError.log";
        /*=== アクセス先がwebサーバーかチェック ===*/
        public static bool WebServer = false;
        /*=== サーバーのURL ===*/
        public static string IpAddress = "";
        /*=== サーバーのダウンロードディレクトリー ===*/
        public static string FilePass = "";
        /*=== Uploadするソフトの名前 ===*/
        public static string ExeNeme = "";
        /*=== Revision管理ファイルの名前 ===*/
        public static string NewRevFile = "";
        /*=== 消去除外フォルダー ===*/
        public static string KeepFolder = "";
        /*=== Uploadするソフトの再起動確認 ===*/
        public static bool SoftReStart = false;
        /*=== pingウェイト用_ms ===*/
        public static int PingWait = 0;


        /*=== 設定無し ===*/
        /*=== 通信確認用 ===*/
        public static bool IPContest = false;
        /*=== バージョン確認用 ===*/
        public static bool VerCheckResult = false;//まだ未使用


        public void GetConfig()
        {/*=== インポート開始 ===*/

            /*=== バージョン名（AssemblyVersion属性）を取得 ===*/
            System.Reflection.Assembly mainAssembly = System.Reflection.Assembly.GetEntryAssembly();
            System.Reflection.AssemblyName mainAssemName = mainAssembly.GetName();
            Version appVersion = mainAssemName.Version;
            /*=== END_バージョン名（AssemblyVersion属性）を取得 ===*/

            try
            {

                if (File.Exists(@".\" + ConfFile))
                {/*=== 設定ファイル読み込み開始 ===*/

                    /* XmlSerializerオブジェクトを作成 */
                    System.Xml.Serialization.XmlSerializer serializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(UpdaterConfig.configuration));
                    /* 読み込むファイルを開く */
                    StreamReader sr = new StreamReader(
                        ConfFile, new System.Text.UTF8Encoding(false));
                    /* XMLファイルから読み込み、逆シリアル化する */
                    UpdaterConfig.configuration obj = (UpdaterConfig.configuration)serializer.Deserialize(sr);

                    /*=== アクセス先がwebサーバーかチェック ===*/
                    WebServer = obj.WebServer;
                    /*=== サーバーのURL ===*/
                    IpAddress = obj.IpAddress;
                    /*=== サーバーのダウンロードディレクトリー ===*/
                    FilePass = obj.FilePass;
                    /*=== Uploadするソフトの名前 ===*/
                    ExeNeme = obj.ExeNeme;
                    /*=== Revision管理ファイルの名前 ===*/
                    NewRevFile = obj.NewRevFile;
                    /*=== 消去除外フォルダー ===*/
                    KeepFolder = obj.KeepFolder;
                    /*=== Uploadするソフトの再起動確認 ===*/
                    SoftReStart = obj.SoftReStart;
                    /*=== pingウェイト用_ms ===*/
                    PingWait = obj.PingWait;

                    /* ファイルを閉じる */
                    sr.Close();

                }//*=== END_設定ファイル読み込み開始 ===*/
                else
                {/*=== 設定ファイルが無い ===*/

#if DEBUG
                    Console.WriteLine(mainAssemName.Name + " : ver." + appVersion + " (Now DebugMode)");
#else
            Console.WriteLine(mainAssemName.Name + " : ver." + appVersion);
#endif

                    Console.WriteLine("Error : There is no \"" + ConfFile + "\" the updater configuration file.");
                    Console.WriteLine("Process : The program has been terminated.");
                    /*=== エンターキー待ち ===*/
                    Console.WriteLine("Press \"Enter\"to exit");
                    Console.ReadKey();
                    Environment.Exit(0);

                }/*=== END_設定ファイルが無い ===*/

            }
            catch (Exception)
            {
#if DEBUG
                Console.WriteLine(mainAssemName.Name + " : ver." + appVersion + " (Now DebugMode)");
#else
            Console.WriteLine(mainAssemName.Name + " : ver." + appVersion);
#endif
                Console.WriteLine("Warning : An exception occurred during import. Check the configuration file.");
                Console.WriteLine("Process : The program has been terminated.");

                /*=== エンターキー待ち ===*/
                Console.WriteLine("Press \"Enter\"to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

        }/*=== END_インポート開始 ===*/

    }/*=== END_設定用変数 ===*/


    class Program
    {/*=== メイン処理 ===*/

        /*********************************************************************
        *  メイン処理
        *********************************************************************/
        static void Main(string[] args)
        {/*=== メインルーチン ===*/

            /*=== プログラム処理を生成 ===*/
            Main_Program main_Program = new Main_Program();
            ini_Settings ini_Settings = new ini_Settings();
            Main_Program.Logger logger = new Main_Program.Logger();

            ini_Settings.GetConfig();

            if (ini_Settings.WebServer == true)
            {/*=== Webserverはまだない ===*/
                logger.log("Error : Web server configuration does not yet exist.");
                logger.log("Process : The program has been terminated.");
                /*=== エンターキー待ち ===*/
                logger.log("Press \"Enter\"to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }/*=== END_Webserverはまだない ===*/


            /*=== エラーログ定義 ===*/
            var LogFolde = @".\OutputLog\";


            /*=== アップデート後の処理 ===*/
            if (main_Program.Clean() == true)
            {
#if !DEBUG
                /*=== エラーログの消去 ===*/
                Directory.Delete(LogFolde, true);
#endif
                Environment.Exit(0);
            }


            if (!Directory.Exists(LogFolde))
            {/*=== OutputLogのフォルダー作成 ===*/
                Directory.CreateDirectory(LogFolde);
            }/*=== END_OutputLogのフォルダー作成 ===*/
            else
            {/*=== OutputLogのフォルダーの再作成 ===*/
                main_Program.Delete(LogFolde);
                Directory.CreateDirectory(LogFolde);
            }/*=== END_OutputLogのフォルダーの再作成 ===*/

            /*=== バージョン名（AssemblyVersion属性）を取得 ===*/
            System.Reflection.Assembly mainAssembly = System.Reflection.Assembly.GetEntryAssembly();
            System.Reflection.AssemblyName mainAssemName = mainAssembly.GetName();
            Version appVersion = mainAssemName.Version;

#if DEBUG
            logger.log(mainAssemName.Name + " : ver." + appVersion + " (Now DebugMode)");
#else
            logger.log(mainAssemName.Name + " : ver." + appVersion);
#endif
            /*=== END_バージョン名（AssemblyVersion属性）を取得 ===*/


            /*=== ソフトの現状確認 ===*/
            var version = main_Program.ExeAsmCheck();
            logger.log("");

            /*=== サーバーとの通信確認 ===*/
            logger.log("Process : Server connection check");
            logger.log("");
            if (main_Program.ServerPing() == false)
            {
                logger.log("Message : Not connected to network.");
                logger.log("Process : The program has been terminated.");
                Environment.Exit(0);
            }

            logger.log("Message : The network was able to confirm the connection.");
            logger.log("");
            /*=== END_サーバーとの通信確認 ===*/


            /*=== ソフトの最新ver確認 ===*/
            var New_version = main_Program.NweVersion();
            if (New_version == "Not")
            {
                logger.log("Process : The program has been terminated.");
                Environment.Exit(0);
            }

            /*=== ソフトの比較 ===*/
            if (false == main_Program.isNew(version, New_version))
            {
                logger.log("Result : Ver. \"" + version + "\"is the new.");
                logger.log("Message : Finish checking for updates.");
                logger.log("Process : The program has been terminated.");
                Environment.Exit(0);
            }

            logger.log("Message : Update because there is the new ver." + New_version);
            logger.log("");


            /*=== ソフトを閉じる ===*/
            logger.log("Message : Confirm \"" + ini_Settings.ExeNeme + "\" shutdown.");
            logger.log("");
            if (main_Program.End_Soft() == false)
            {
                logger.log("Message : Finish checking for updates.");
                logger.log("Process : The program has been terminated.");
                Environment.Exit(0);
            }
            logger.log("");


            /*=== ソフトアップデート開始 ===*/
            if (main_Program.UpdateExe(version, New_version) == true)
            {
                Environment.Exit(0);
            }

            logger.log("");
            logger.log("Process : The program has been terminated.");

            /*=== エンターキー待ち ===*/
            logger.log("Press \"Enter\"to exit");
            Console.ReadKey();

        }/*=== End_メインルーチン ===*/
    }/*=== END_メイン処理 ===*/

    class Main_Program
    {/*=== プログラム処理 ===*/

        /*=== プログラム処理を生成 ===*/
        ini_Settings ini_Settings = new ini_Settings();
        Main_Program.Logger logger = new Main_Program.Logger();

        /*********************************************************************
        *   ログ
        *********************************************************************/
        interface ILogger
        {
            void log(String msg);
        }

        public class Logger : ILogger
        {

            /*=== プログラム処理を生成 ===*/
            ini_Settings ini_Settings = new ini_Settings();

            public static String path = "";
            public static String userProfilePath = "";
            public const String encodingName = "UTF-8";
            public StreamWriter sw = null;

            public Logger()
            {
                /*=== エラーログ定義 ===*/
                var LogFolde = @".\OutputLog\";
                userProfilePath = LogFolde;
                String name = ini_Settings.LogFile;
                path = userProfilePath + "\\" + name;

            }

            public void log(String msg)
            {
                if (Directory.Exists(userProfilePath))
                {

                    sw = new StreamWriter(path, true, System.Text.Encoding.GetEncoding(encodingName));

                    Console.WriteLine(msg);
                    sw.WriteLine(msg);
                    sw.Close();

                }
                else
                {
                    Console.WriteLine(msg);
                }
            }

            internal void IPlog(IPStatus status)
            {
                string IPstatus = status.ToString();
                log(status.ToString());

            }

            internal void IP_TTLlog(string v, IPAddress address, int length, long roundtripTime, int ttl)
            {
                string TTLlog = "Reply from " + address
                + ": bytes=" + length
                + " time=" + roundtripTime + "ms"
                + " TTL=" + ttl;
                log(TTLlog);
            }
        }





        /*********************************************************************
        *   Upload後処理
        *********************************************************************/
        public bool Clean()
        {
            if (Environment.CommandLine.IndexOf("/up", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                try
                {
                    string[] args = Environment.GetCommandLineArgs();
                    int pid = Convert.ToInt32(args[2]);
                    Process.GetProcessById(pid).WaitForExit();    // 終了待ち
                }
                catch (Exception)
                {
                }

                var LogFolde = @".\OutputLog\";
                if (!Directory.Exists(LogFolde))
                {/*=== srcのフォルダー作成 ===*/
                    Directory.CreateDirectory(LogFolde);
                }/*=== END_srcのフォルダー作成 ===*/

                logger.log("");
                logger.log("");
                logger.log("=========================================");
                logger.log("Process : Restart \"OK\". ");
                logger.log("=========================================");

                /*=== バージョン名（AssemblyVersion属性）を取得 ===*/
                System.Reflection.Assembly mainAssembly = System.Reflection.Assembly.GetEntryAssembly();
                System.Reflection.AssemblyName mainAssemName = mainAssembly.GetName();
                Version appVersion = mainAssemName.Version;

#if DEBUG
                logger.log(mainAssemName.Name + " : ver." + appVersion + " (Now DebugMode)");
#else
                logger.log(mainAssemName.Name + " : ver." + appVersion);
#endif
                /*=== ソフトの現状確認 ===*/
                ExeAsmCheck();
                logger.log("");

                logger.log("Message : Erase the \"" + mainAssemName.Name + ".exe.old" + "\"fail");
                File.Delete(mainAssemName.Name + ".exe.old");
                logger.log("Process : End Upload exe");
                return true;
            }
            return false;
        }


        /*********************************************************************
        *   ソフトを閉じる
        *********************************************************************/
        public bool End_Soft()
        {/*=== ソフトを閉じる ===*/
            bool End_exe = false;

            logger.log("Process : " + ini_Settings.ExeNeme + " shutdown...");

            if (Process.GetProcessesByName(ini_Settings.ExeNeme).Length > 0)
            {

                //Everestのプロセスを取得
                Process[] ps =
                    Process.GetProcessesByName(ini_Settings.ExeNeme);

                foreach (Process p in ps)
                {

                    //メッセージを閉じる
                    logger.log("Now Kill " + ini_Settings.ExeNeme + "...");
                    p.Kill();

                    //プロセスが終了するまで最大30秒待機する
                    p.WaitForExit(30000);

                    //プロセスが終了したか確認する

                    if (p.HasExited)
                    {
                        logger.log(ini_Settings.ExeNeme + " shutdown = OK.");
                        End_exe = true;
                        return End_exe;
                    }
                    else
                    {
                        logger.log("Error : " + ini_Settings.ExeNeme + " failed to finish.");
                        End_exe = false;
                    }

                }

            }
            else
            {
                logger.log(ini_Settings.ExeNeme + " was not running = OK.");
                End_exe = true;
            }

            return End_exe;
        }/*=== END_ソフトを閉じる ===*/


        /*********************************************************************
        *   ソフトの起動
        *********************************************************************/
        public void SoftStart()
        {/*=== ソフトウェア起動 ===*/
            try
            {

                //Processオブジェクトを作成する
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                //起動する実行ファイルのパスを設定する
                p.StartInfo.FileName = @".\" + ini_Settings.ExeNeme + ".exe";
                p.StartInfo.Verb = "RunAs";
                p.StartInfo.UseShellExecute = true;
                //起動する。プロセスが起動した時はTrueを返す。
                bool result = p.Start();
            }
            catch
            {/* 例外処理 */
                logger.log("Error : \"" + ini_Settings.ExeNeme + ".exe\" could not be accessed.");
            }

        }/*=== END_ソフトウェア起動 ===*/

        /*********************************************************************
        *   通信確認
        *********************************************************************/


        public bool ServerPing()
        {/*=== 通信確認 ===*/

            try
            {

                logger.log("IP Address : " + ini_Settings.IpAddress);
                logger.log("Ping Send Wait Time : " + ini_Settings.PingWait + "ms");
                Ping sender = new Ping();
                for (int i = 0; i < 4; i++)
                {
                    PingReply reply = sender.Send(ini_Settings.IpAddress);
                    if (reply.Status == IPStatus.Success)
                    {
                        logger.IP_TTLlog("Reply from {0}: bytes={1} time={2}ms TTL={3}",
                            reply.Address,
                            reply.Buffer.Length,
                            reply.RoundtripTime,
                            reply.Options.Ttl);

                        if (reply.Options.Ttl >= 1)
                        {
                            ini_Settings.IPContest = true;
                        }
                        else
                        {
                            ini_Settings.IPContest = false;
                        }

                    }
                    else
                    {
                        ini_Settings.IPContest = false;
                        logger.IPlog(reply.Status);
                    }

                    // ping送信の間隔を取る
                    if (i < 3)
                    {
                        System.Threading.Thread.Sleep(ini_Settings.PingWait);
                    }
                }
            }
            catch
            {
                logger.log("Exception : Make sure the IP or network connection is correct.");
                ini_Settings.IPContest = false;
            }

            return ini_Settings.IPContest;

        }/*=== END_通信確認 ===*/


        /*********************************************************************
        *   ソフトの現状確認
        *********************************************************************/
        public string ExeAsmCheck()
        {/*=== ソフトの現状確認 ===*/
            // ファイル名を指定して、そのファイルのバージョンを取得する方法
            var filename = @".\" + ini_Settings.ExeNeme + ".exe";
            if (File.Exists(filename))
            {
                FileVersionInfo filever = FileVersionInfo.GetVersionInfo(filename);
                logger.log(ini_Settings.ExeNeme + " : ver." + filever.FileVersion);
                return filever.FileVersion;
            }
            else
            {
                return "";
            }


        }/*=== END_ソフトの現状確認 ===*/


        /*********************************************************************
        *  ソフトのアップデート
        *********************************************************************/
        public bool UpdateExe(string version, string New_version)
        {/*=== ソフトのアップデート ===*/

            /*=== 新しい実行ファイルの場所 ===*/
            string NweExe;

            var DownloadPass = @"\\" + ini_Settings.IpAddress + @"\" + ini_Settings.FilePass + @"\";
            var NewFolder_version = ini_Settings.ExeNeme + "_" + New_version.Replace(".", "_");

            var RootFolde = @".\src";
            if (!Directory.Exists(RootFolde))
            {/*=== srcのフォルダー作成 ===*/
                logger.log("Message : New Folder \"" + RootFolde + "\"");
                Directory.CreateDirectory(RootFolde);
            }/*=== END_srcのフォルダー作成 ===*/
            else
            {/*=== srcのフォルダー消去 ===*/
                logger.log("Message : Delete Folder \"" + RootFolde + "\"");
                Delete(RootFolde);
                logger.log("Message : New Folder \"" + RootFolde + "\"");
                Directory.CreateDirectory(RootFolde);
            }/*=== END_srcのフォルダー消去 ===*/

            /*=== ダウンロードのファイル名 ===*/
            var NweDownloadFolder = RootFolde + @"\" + NewFolder_version;
            /*=== 解凍先 ===*/
            RootFolde = RootFolde + @"\" + NewFolder_version + ".zip";
            /*=== ダウンロードファイル ===*/
            var DownloadZip = DownloadPass + NewFolder_version + ".zip";


            if (File.Exists(DownloadZip))
            {/*=== Zipファイル確認 ===*/
                using (var wclient = new WebClient())
                {
                    logger.log("Message : Download File \"" + DownloadZip + "\"");
                    logger.log("");
                    wclient.DownloadFile(DownloadZip, RootFolde);
                }

                logger.log("Message : Open the Zip file to a \"" + NweDownloadFolder + "\"");
                logger.log("");
                ZipFile.ExtractToDirectory(RootFolde, NweDownloadFolder);

                logger.log("Message : Erase the \"" + NewFolder_version + ".zip" + "\"file");
                File.Delete(RootFolde);


                /*=== フォルダーの検索 ===*/
                /* 元パスの存在確認 */
                if (Directory.Exists(@".\src"))
                {/* 元パスは存在する */

                    /*=== 解凍ファイル移動 ===*/
                    NweExe = LookFor(@".\src", ini_Settings.ExeNeme + ".exe"); ;

                    if (NweExe != null)
                    {/*===実行exe確認 ===*/

                        if (NweExe == "")
                        {/*===実行exe無し ===*/
                            logger.log("Error : Missing copy source address of \"" + ini_Settings.ExeNeme + ".exe\".");
                            return false;
                        }/*===END_実行exe確認 ===*/

                    }/*===実行exe確認 ===*/
                    else
                    {/*===実行exe無し ===*/
                        logger.log("Error : Missing copy source address of \"" + ini_Settings.ExeNeme + ".exe\".");
                        return false;
                    }/*===END_実行exe確認 ===*/

                }
                else
                {/* 元パスはしない */

                    logger.log("Error : Missing file. \".\"src \"");
                    return false;

                }/*=== END_フォルダーの確認 ===*/
                /*=== END_フォルダーの検索 ===*/


                /*=== バージョン名（AssemblyVersion属性）を取得 ===*/
                System.Reflection.Assembly mainAssembly = System.Reflection.Assembly.GetEntryAssembly();
                System.Reflection.AssemblyName mainAssemName = mainAssembly.GetName();
                /*=== 自分自身のファイル名 ===*/
                string OldExe = mainAssemName.Name + ".exe.old";

                if (File.Exists(OldExe))
                {/*=== exe.old確認 ===*/
                    logger.log("Message : Removed old \"" + OldExe + "\".");
                    File.Delete(OldExe);
                }/*=== END_exe.old確認 ===*/

                File.Move(mainAssemName.Name + ".exe", OldExe);

                /*=== ディレクトリー内のファイル消去 ===*/
                OldExe = @".\" + OldExe;
                string OldAdb = @".\" + mainAssemName.Name + ".pdb";

                string[] filePaths = Directory.GetFiles(@".\");
                logger.log("Message : Erase old files.");
                foreach (string filePath in filePaths)
                {/*=== ディレクトリーファイル消去 ===*/
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    if (filePath != OldExe)
                    {
                        if (filePath != OldAdb)
                        {
                            logger.log("Delete : \"" + filePath + "\"");
                            File.Delete(filePath);
                        }
                    }

                }/*=== ディレクトリーファイル消去 ===*/

                string[] FolderPaths = Directory.GetDirectories(@".\");
                logger.log("Message : Erase old Folder.");
                foreach (string folderPath in FolderPaths)
                {/*=== ディレクトリーフォルダー消去 ===*/
                    File.SetAttributes(folderPath, FileAttributes.Normal);
                    if (folderPath != @".\src")
                    {
                        if (folderPath != @".\OutputLog")
                        {
                            if (folderPath != @".\" + ini_Settings.KeepFolder)
                            {
                                logger.log("Delete : \"" + folderPath + "\"");
                                Delete(folderPath);
                            }
                        }
                    }

                }/*=== END_ディレクトリーフォルダー消去 ===*/

                logger.log("Message : Finished deleting old folder.");
                logger.log("");
                
                /*=== 解凍フォルダー移動 ===*/
                logger.log("Move the unzipped new file to the directory.");
                DirectoryCopy(NweExe, @".\");
                logger.log("Message : Finished moving new file to directory.");
                logger.log("");

                /*=== フォルダ削除 ===*/
                Directory.Delete(@".\src", true);
                logger.log("Delete : \"" + @".\src" + "\"");

                /*=== アップデート後確認 ===*/
                /*=== ソフトの比較 ===*/

                var Download_version = ExeAsmCheck(); ;

                if (false == New_isNew(New_version, Download_version))
                {
                    logger.log("warning : The downloaded \"" + ini_Settings.ExeNeme + " Ver." + Download_version + "\" is old.");
                    logger.log("");
                }
                else
                {
                    logger.log("Result : \"" + ini_Settings.ExeNeme + " Ver." + Download_version + "\" is the new.");
                    logger.log("");
                }


                if (ini_Settings.SoftReStart == true)
                {/*=== ソフトウェア起動 ===*/
                    logger.log("");
                    logger.log("Message : Start \"" + ini_Settings.ExeNeme + ".exe\".");
                    SoftStart();
                }/*=== END_ソフトウェア起動 ===*/

                logger.log("Process : \"" + mainAssemName.Name + "\" Re boot.");
                Process.Start(mainAssemName.Name + ".exe", "/up " + Process.GetCurrentProcess().Id);
                return true;

            }/*=== END_Zipファイル確認 ===*/
            else
            {/*=== Zipファイルが存在しない ===*/
                logger.log("Error : Missing Zip path. \"" + DownloadZip + "\"");
                return false;
            }/*=== END_Zipファイルが存在しない ===*/

        }/*=== END_ソフトのアップデート ===*/


        public string NweVersion()
        {/*=== 最新バージョン情報の取得 ===*/

            var New_FilePass = @"\\" + ini_Settings.IpAddress + @"\" + ini_Settings.FilePass + @"\";
            var Text = string.Empty;

            /*=== フォルダーの確認 ===*/
            /* 元パスの存在確認 */
            if (Directory.Exists(New_FilePass))
            {/* 元パスは存在する */

                /*=== 最新バージョンリストの確認 ===*/
                if (File.Exists(New_FilePass + ini_Settings.NewRevFile))
                {/* 元フォルダは存在する */

                    using (var reader = new StreamReader(New_FilePass + ini_Settings.NewRevFile))
                    {
                        Text = reader.ReadToEnd();
                    }

                    if (Text == null)
                    {
                        Text = "Not";
                    }

                    if (Text == "")
                    {
                        Text = "Not";
                    }


                }/* END_元フォルダは存在する */
                else
                {/* 元フォルダが無い */

                    logger.log("Error : Missing file. \"" + ini_Settings.NewRevFile + "\"");
                    Text = "Not";

                }/* END_元フォルダが無い */

            }/* END_元パスは存在する */
            else
            {/* 元パスが無い */

                logger.log("Error : Missing folder path. \"" + New_FilePass + "\"");
                Text = "Not";

            }/* END_元パスが無い */



            return Text;
        }/*=== END_最新バージョン情報の取得 ===*/

        public bool isNew(string current, string target)
        {/*=== バージョン確認 ===*/
            var ca = current.Split('.');
            var ta = target.Split('.');
            var len = Math.Min(ca.Length, ta.Length);

            for (var i = 0; i < len; i++)
            {
                int ci, ti;
                if (!int.TryParse(ca[i], out ci) | !int.TryParse(ta[i], out ti))
                {
                    return false;
                }

                if (ci < ti)
                {
                    return true;
                }
                if (ci > ti)
                {
                    return false;
                }
            }

            return ca.Length < ta.Length;
        }/*=== END_バージョン確認 ===*/


        public bool New_isNew(string current, string target)
        {/*=== バージョン一致確認 ===*/
            var ca = current.Split('.');
            var ta = target.Split('.');
            var len = Math.Min(ca.Length, ta.Length);

            for (var i = 0; i < len; i++)
            {
                int ci, ti;
                if (!int.TryParse(ca[i], out ci) | !int.TryParse(ta[i], out ti))
                {
                    return true;
                }

                if (ci < ti)
                {
                    return false;
                }
                if (ci > ti)
                {
                    return false;
                }
            }

            return ca.Length < ta.Length;
        }/*=== バージョン一致確認 ===*/

        public void Delete(string targetDirectoryPath)
        {/*=== 指定したディレクトリとその中身を全て削除する ===*/

            try
            {
                if (!Directory.Exists(targetDirectoryPath))
                {
                    return;
                }

                /*=== ディレクトリ以外の全ファイルを削除 ===*/
                string[] filePaths = Directory.GetFiles(targetDirectoryPath);
                foreach (string filePath in filePaths)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    logger.log("Delete : \"" + filePath + "\"");
                    File.Delete(filePath);
                }

                /*=== ディレクトリの中のディレクトリも再帰的に削除 ===*/
                string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
                foreach (string directoryPath in directoryPaths)
                {
                    logger.log("Delete : \"" + directoryPath + "\"");
                    Delete(directoryPath);
                }

                /*=== 中が空になったらディレクトリ自身も削除 ===*/
                logger.log("Delete : \"" + targetDirectoryPath + "\"");
                Directory.Delete(targetDirectoryPath, false);
            }
            catch
            {
                logger.log("Error : Not delete \"" + targetDirectoryPath + "\"");
                return;
            }

        }/*=== 指定したディレクトリとその中身を全て削除する ===*/


        public static void DirectoryCopy(string sourcePath, string destinationPath)
        {/*=== ディレクトリのコピー ===*/
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destinationPath);
            Main_Program.Logger logger = new Main_Program.Logger();

            /*=== コピー先のディレクトリがなければ作成する ===*/
            if (destinationDirectory.Exists == false)
            {
                destinationDirectory.Create();
                logger.log("Move : \"" + sourceDirectory.Attributes + "\"");
                destinationDirectory.Attributes = sourceDirectory.Attributes;
            }

            /*=== ファイルのコピー ===*/
            foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
            {
                //同じファイルが存在していたら、常に上書きする ===*/
                logger.log("Move : \"" + fileInfo.Name + "\"");
                fileInfo.CopyTo(destinationDirectory.FullName + @"\" + fileInfo.Name, true);
            }

            /*=== ディレクトリのコピー（再帰を使用） ===*/
            foreach (System.IO.DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
            {
                logger.log("Move : \"" + directoryInfo.Name + "\"");
                DirectoryCopy(directoryInfo.FullName, destinationDirectory.FullName + @"\" + directoryInfo.Name);
            }
        }/*=== END_ディレクトリのコピー ===*/


        public string LookFor(string LookPass, string Loolexe )
        {/*=== 検索する ===*/

            IEnumerable<string> files = Directory.EnumerateFiles(LookPass, Loolexe, SearchOption.AllDirectories);

            foreach (string str in files)
            {
                Loolexe = str;
            }

            Loolexe = Path.GetDirectoryName(Loolexe);


            logger.log("Copy source address of exe: \"" + Loolexe + "\"");
            return Loolexe;
        }/*=== 検索する ===*/


    }/*=== END_プログラム処理 ===*/

}/*=== End_コンソールアプリケーション ===*/
