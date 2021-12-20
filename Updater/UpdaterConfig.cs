using System;

public class UpdaterConfig
{

    // メモ: 生成されたコードは、少なくとも .NET Framework 4.5または .NET Core/Standard 2.0 が必要な可能性があります。
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class configuration
    {

        public bool webServerField;

        public string ipAddressField;

        public string filePassField;

        public string exeNemeField;

        public string newRevFileField;

        public string keepFolderField;

        public bool softReStartField;

        public ushort pingWaitField;

        /// <remarks/>
        public bool WebServer
        {
            get
            {
                return this.webServerField;
            }
            set
            {
                this.webServerField = value;
            }
        }

        /// <remarks/>
        public string IpAddress
        {
            get
            {
                return this.ipAddressField;
            }
            set
            {
                this.ipAddressField = value;
            }
        }

        /// <remarks/>
        public string FilePass
        {
            get
            {
                return this.filePassField;
            }
            set
            {
                this.filePassField = value;
            }
        }

        /// <remarks/>
        public string ExeNeme
        {
            get
            {
                return this.exeNemeField;
            }
            set
            {
                this.exeNemeField = value;
            }
        }

        /// <remarks/>
        public string NewRevFile
        {
            get
            {
                return this.newRevFileField;
            }
            set
            {
                this.newRevFileField = value;
            }
        }

        /// <remarks/>
        public string KeepFolder
        {
            get
            {
                return this.keepFolderField;
            }
            set
            {
                this.keepFolderField = value;
            }
        }


        /// <remarks/>
        public bool SoftReStart
        {
            get
            {
                return this.softReStartField;
            }
            set
            {
                this.softReStartField = value;
            }
        }

        /// <remarks/>
        public ushort PingWait
        {
            get
            {
                return this.pingWaitField;
            }
            set
            {
                this.pingWaitField = value;
            }
        }
    }


}
