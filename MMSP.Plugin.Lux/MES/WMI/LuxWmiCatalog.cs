namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// V5.0.5 目录中的 WMI 方法，并补上仅 V5.0.3 有的方法。
    /// 第 29 章的工位调用顺序不在这里自动执行。已取消、未启用和已停产的方法仍保留。
    /// </summary>
    public static class LuxWmiCatalog
    {
        /// <summary>
        /// 检查是否允许过站。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6011 = new("MES_BA_WI6011", "检查是否允许过站", "", ["Stationid", "BarCode", "Mo", "LineNumber"], false);

        /// <summary>
        /// 检查是否允许过站--针对点胶、预加工。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601L = new("MES_BA_WI601L", "检查是否允许过站--针对点胶、预加工", "", ["Stationid", "BarCode", "Mo", "LineNumber"], false);

        /// <summary>
        /// 检查是否允许过站针对笔电混投。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601K = new("MES_BA_WI601K", "检查是否允许过站针对笔电混投", "", ["BarCode", "HostName", "Mac", "Stationid", "LineNumber"], false);

        /// <summary>
        /// 检查是否允许过站针对服务器项目。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601S = new("MES_BA_WI601S", "检查是否允许过站针对服务器项目", "", ["BarCode", "HostName", "Mac", "Stationid", "LineNumber"], false);

        /// <summary>
        /// 各站点过站处理,已经取消处理。已取消。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6012 = new("MES_BA_WI6012", "各站点过站处理,已经取消处理", "已取消", [], false);

        /// <summary>
        /// 验证账号密码是否正确。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6013 = new("MES_BA_WI6013", "验证账号密码是否正确", "", ["Account", "Password"], false);

        /// <summary>
        /// 验证账号是否有对应模块的权限。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6014 = new("MES_BA_WI6014", "验证账号是否有对应模块的权限", "", ["Account", "FormName", "Auth", "Line", "StationName"], false);

        /// <summary>
        /// MES_BA_WI6015。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6015 = new("MES_BA_WI6015", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6016。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6016 = new("MES_BA_WI6016", "", "", [], false);

        /// <summary>
        /// 获取工厂。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6017 = new("MES_BA_WI6017", "获取工厂", "", [], true);

        /// <summary>
        /// MES_BA_WI6018。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6018 = new("MES_BA_WI6018", "", "", ["testWorkID"], true);

        /// <summary>
        /// MES_BA_WI6019。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6019 = new("MES_BA_WI6019", "", "", ["testStationID"], true);

        /// <summary>
        /// 已经取消处理。已取消。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601A = new("MES_BA_WI601A", "已经取消处理", "已取消", [], false);

        /// <summary>
        /// MES_BA_WI601B。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601B = new("MES_BA_WI601B", "", "", [], true);

        /// <summary>
        /// MES_BA_WI601C。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601C = new("MES_BA_WI601C", "", "", ["ProgramName"], true);

        /// <summary>
        /// MES_BA_WI601D。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601D = new("MES_BA_WI601D", "", "", [], true);

        /// <summary>
        /// MES_BA_WI601E。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601E = new("MES_BA_WI601E", "", "", ["imei"], false);

        /// <summary>
        /// 目前未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601E1 = new("MES_BA_WI601E1", "目前未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI601F。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601F = new("MES_BA_WI601F", "", "", [], true);

        /// <summary>
        /// MES_BA_WI601G。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601G = new("MES_BA_WI601G", "", "", ["PcbSN", "ShipID"], true);

        /// <summary>
        /// MES_BA_WI601H。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601H = new("MES_BA_WI601H", "", "", ["imei"], true);

        /// <summary>
        /// 目前已停产。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601I = new("MES_BA_WI601I", "目前已停产", "已停产", [], false);

        /// <summary>
        /// MES_BA_WI601M。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601M = new("MES_BA_WI601M", "", "", [], true);

        /// <summary>
        /// 目前已停产。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601N = new("MES_BA_WI601N", "目前已停产", "已停产", [], false);

        /// <summary>
        /// MES_BA_WI601KD。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601KD = new("MES_BA_WI601KD", "", "", [], false);

        /// <summary>
        /// MES_BA_WI601_QC。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601_QC = new("MES_BA_WI601_QC", "", "", ["FPSN", "Content"], true);

        /// <summary>
        /// MES_BA_WI601Q。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601Q = new("MES_BA_WI601Q", "", "", ["BarCode", "Station"], true);

        /// <summary>
        /// MES_BA_WI601R。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601R = new("MES_BA_WI601R", "", "", [], true);

        /// <summary>
        /// MES_BA_WI601W。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601W = new("MES_BA_WI601W", "", "", ["Account", "Password"], true);

        /// <summary>
        /// MES_BA_WI601T。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601T = new("MES_BA_WI601T", "", "", ["BarCode", "BarCodeType"], true);

        /// <summary>
        /// MES_BA_WI601U。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601U = new("MES_BA_WI601U", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_BA_WI601V。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI601V = new("MES_BA_WI601V", "", "", ["BarCode", "Station"], true);

        /// <summary>
        /// MES_BA_WI6021。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6021 = new("MES_BA_WI6021", "", "", [], false);

        /// <summary>
        /// MES_BA_WI602L。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602L = new("MES_BA_WI602L", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6022。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6022 = new("MES_BA_WI6022", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6023。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6023 = new("MES_BA_WI6023", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6023_GetConnection_Internal。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6023_GetConnection_Internal = new("MES_BA_WI6023_GetConnection_Internal", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6024。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6024 = new("MES_BA_WI6024", "", "", [], false);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6024_V2 = new("MES_BA_WI6024_V2", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI6025。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6025 = new("MES_BA_WI6025", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6026。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6026 = new("MES_BA_WI6026", "", "", ["baj_c02", "bag_c01"], true);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6026_KD = new("MES_BA_WI6026_KD", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI6027。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6027 = new("MES_BA_WI6027", "", "", ["mac", "hostName"], false);

        /// <summary>
        /// MES_BA_WI6028。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6028 = new("MES_BA_WI6028", "", "", ["baj_c02"], true);

        /// <summary>
        /// MES_BA_WI6029。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6029 = new("MES_BA_WI6029", "", "", ["ProgramName"], false);

        /// <summary>
        /// MES_BA_WI602A。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602A = new("MES_BA_WI602A", "", "", ["FpSN", "StationID"], true);

        /// <summary>
        /// MES_BA_WI602B。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602B = new("MES_BA_WI602B", "", "", ["FpSN", "StationID"], true);

        /// <summary>
        /// MES_BA_WI602C。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602C = new("MES_BA_WI602C", "", "", [], true);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602D = new("MES_BA_WI602D", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI602E。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602E = new("MES_BA_WI602E", "", "", ["LINE_NAME"], false);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI602ALL = new("MES_BA_WI602ALL", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI60210。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI60210 = new("MES_BA_WI60210", "", "", ["MAC", "StationID"], false);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI60211 = new("MES_BA_WI60211", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI6030。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6030 = new("MES_BA_WI6030", "", "", ["baj_c02"], true);

        /// <summary>
        /// MES_BA_WI6031。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6031 = new("MES_BA_WI6031", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6032。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6032 = new("MES_BA_WI6032", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6033。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6033 = new("MES_BA_WI6033", "", "", ["messid"], true);

        /// <summary>
        /// MES_BA_WI6034。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6034 = new("MES_BA_WI6034", "", "", ["Mo", "StationID"], true);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6035 = new("MES_BA_WI6035", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI6036。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6036 = new("MES_BA_WI6036", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6041。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6041 = new("MES_BA_WI6041", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6041_EXT_BLL = new("MES_BA_WI6041_EXT_BLL", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI6042。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6042 = new("MES_BA_WI6042", "", "", [], true);

        /// <summary>
        /// MES_BA_WI604D。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604D = new("MES_BA_WI604D", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6043。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6043 = new("MES_BA_WI6043", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6044。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6044 = new("MES_BA_WI6044", "", "", ["KeyPartsLink", "S_ID", "S_NM", "PSN", "KP_NM", "KP_Value", "KP_PN", "KP_CustPN", "KP_Note", "T_Line", "T_Order"], false);

        /// <summary>
        /// MES_BA_WI6046。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6046 = new("MES_BA_WI6046", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI6048。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6048 = new("MES_BA_WI6048", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604B。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604B = new("MES_BA_WI604B", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604C。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604C = new("MES_BA_WI604C", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI6049。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6049 = new("MES_BA_WI6049", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604E。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604E = new("MES_BA_WI604E", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604A。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604A = new("MES_BA_WI604A", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604F。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604F = new("MES_BA_WI604F", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604W。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604W = new("MES_BA_WI604W", "", "", ["content", "snresult", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "itemresult", "specialitem", "writeNumberList", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_BA_WI604H。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604H = new("MES_BA_WI604H", "", "", [], true);

        /// <summary>
        /// MES_BA_WI604BU。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604BU = new("MES_BA_WI604BU", "", "", [], true);

        /// <summary>
        /// MES_BA_WI604HW。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604HW = new("MES_BA_WI604HW", "", "", [], true);

        /// <summary>
        /// MES_BA_WI604O。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI604O = new("MES_BA_WI604O", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6051。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6051 = new("MES_BA_WI6051", "", "", [], false);

        /// <summary>
        /// 孟加拉，国内不用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6052 = new("MES_BA_WI6052", "孟加拉，国内不用", "孟加拉，国内不用", [], false);

        /// <summary>
        /// HWPAD返回维修表的是否有input状态的记录。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6054 = new("MES_BA_WI6054", "HWPAD返回维修表的是否有input状态的记录", "", ["FpSN"], true);

        /// <summary>
        /// HWPAD检查CSN发料是否在工单的BOM表中。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6055 = new("MES_BA_WI6055", "HWPAD检查CSN发料是否在工单的BOM表中", "", ["FpSN"], true);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6061 = new("MES_BA_WI6061", "未启用", "未启用", [], false);

        /// <summary>
        /// MES_BA_WI6064。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6064 = new("MES_BA_WI6064", "", "", ["FpSN"], true);

        /// <summary>
        /// MES_BA_WI6071。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6071 = new("MES_BA_WI6071", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6072。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6072 = new("MES_BA_WI6072", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6073。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6073 = new("MES_BA_WI6073", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6074。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6074 = new("MES_BA_WI6074", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6075。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6075 = new("MES_BA_WI6075", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6076。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6076 = new("MES_BA_WI6076", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6077。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6077 = new("MES_BA_WI6077", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078 = new("MES_BA_WI6078", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogBefore = new("MES_BA_WI6078_LogBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_Log。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_Log = new("MES_BA_WI6078_Log", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogCreateBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogCreateBefore = new("MES_BA_WI6078_LogCreateBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogCreate。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogCreate = new("MES_BA_WI6078_LogCreate", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogAddPcbaBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogAddPcbaBefore = new("MES_BA_WI6078_LogAddPcbaBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogAddPcba。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogAddPcba = new("MES_BA_WI6078_LogAddPcba", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogGetPcbaBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogGetPcbaBefore = new("MES_BA_WI6078_LogGetPcbaBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_LogGetPcba。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_LogGetPcba = new("MES_BA_WI6078_LogGetPcba", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6078_SourcPSN。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_SourcPSN = new("MES_BA_WI6078_SourcPSN", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6078_ASSYBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_ASSYBefore = new("MES_BA_WI6078_ASSYBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_ASSY。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_ASSY = new("MES_BA_WI6078_ASSY", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_ByAllWo。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_ByAllWo = new("MES_BA_WI6078_ByAllWo", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_ByWo。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_ByWo = new("MES_BA_WI6078_ByWo", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_SMTBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_SMTBefore = new("MES_BA_WI6078_SMTBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_SMT。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_SMT = new("MES_BA_WI6078_SMT", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_TSP。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_TSP = new("MES_BA_WI6078_TSP", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_TSPByPCBSN。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_TSPByPCBSN = new("MES_BA_WI6078_TSPByPCBSN", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_SpListBefore。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_SpListBefore = new("MES_BA_WI6078_SpListBefore", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6078_SpList。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6078_SpList = new("MES_BA_WI6078_SpList", "", "", [], false);

        /// <summary>
        /// MES_BA_WI6079。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6079 = new("MES_BA_WI6079", "", "", [], false);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6081 = new("MES_BA_WI6081", "未启用", "未启用", [], true);

        /// <summary>
        /// MES_BA_WI6082。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6082 = new("MES_BA_WI6082", "", "", [], true);

        /// <summary>
        /// MES_BA_WI6083。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6083 = new("MES_BA_WI6083", "", "", ["material"], true);

        /// <summary>
        /// MES_BA_WI6084。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6084 = new("MES_BA_WI6084", "", "", ["KEY_PART_NO"], true);

        /// <summary>
        /// MES_BA_WI6085。
        /// </summary>
        public static readonly LuxWmiMethod MES_BA_WI6085 = new("MES_BA_WI6085", "", "", ["KEY_PART_NO"], true);

        /// <summary>
        /// MES_SM_WI7011。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7011 = new("MES_SM_WI7011", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7012。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7012 = new("MES_SM_WI7012", "", "", ["PcbSN"], true);

        /// <summary>
        /// MES_SM_WI7013。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7013 = new("MES_SM_WI7013", "", "", ["MO", "PcbSN"], true);

        /// <summary>
        /// MES_SM_WI7014。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7014 = new("MES_SM_WI7014", "", "", ["DN"], true);

        /// <summary>
        /// MES_SM_WI7015。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7015 = new("MES_SM_WI7015", "", "", ["PcbSN"], false);

        /// <summary>
        /// MES_SM_WI7016。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7016 = new("MES_SM_WI7016", "", "", ["PcbSN"], false);

        /// <summary>
        /// MES_SM_WI7017。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7017 = new("MES_SM_WI7017", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7022。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7022 = new("MES_SM_WI7022", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7023。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7023 = new("MES_SM_WI7023", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7024。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7024 = new("MES_SM_WI7024", "", "", [], false);

        /// <summary>
        /// MES_SM_WI702A。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI702A = new("MES_SM_WI702A", "", "", ["PSN"], false);

        /// <summary>
        /// MES_SM_WI7031。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7031 = new("MES_SM_WI7031", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7032。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7032 = new("MES_SM_WI7032", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7021。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7021 = new("MES_SM_WI7021", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7041。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7041 = new("MES_SM_WI7041", "", "", ["Computer", "MAC"], true);

        /// <summary>
        /// MES_SM_WI7042。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7042 = new("MES_SM_WI7042", "", "", ["Barcode"], true);

        /// <summary>
        /// MES_SM_WI7043。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7043 = new("MES_SM_WI7043", "", "", ["SN", "Model"], true);

        /// <summary>
        /// MES_SM_WI7044。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7044 = new("MES_SM_WI7044", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7045。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7045 = new("MES_SM_WI7045", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7046。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7046 = new("MES_SM_WI7046", "", "", ["FixureID"], true);

        /// <summary>
        /// MES_SM_WI7047。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7047 = new("MES_SM_WI7047", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7048。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7048 = new("MES_SM_WI7048", "", "", [], false);

        /// <summary>
        /// MES_SM_WI7049。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7049 = new("MES_SM_WI7049", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_SM_WI7051。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7051 = new("MES_SM_WI7051", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7052。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7052 = new("MES_SM_WI7052", "", "", ["GlueSN"], true);

        /// <summary>
        /// MES_SM_WI7053。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7053 = new("MES_SM_WI7053", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7054。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7054 = new("MES_SM_WI7054", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7055。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7055 = new("MES_SM_WI7055", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7056。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7056 = new("MES_SM_WI7056", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7057。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7057 = new("MES_SM_WI7057", "", "", ["Prodmodel"], true);

        /// <summary>
        /// MES_SM_WI7058。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7058 = new("MES_SM_WI7058", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_SM_WI7059。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7059 = new("MES_SM_WI7059", "", "", [], true);

        /// <summary>
        /// MES_SM_WI705A。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI705A = new("MES_SM_WI705A", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_SM_WI705B。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI705B = new("MES_SM_WI705B", "", "", [], true);

        /// <summary>
        /// MES_SM_WI7060。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7060 = new("MES_SM_WI7060", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_SM_WI706A。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI706A = new("MES_SM_WI706A", "", "", [], false);

        /// <summary>
        /// MES_SM_WI706B。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI706B = new("MES_SM_WI706B", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_SM_WI706C。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI706C = new("MES_SM_WI706C", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_SM_WI7070。
        /// </summary>
        public static readonly LuxWmiMethod MES_SM_WI7070 = new("MES_SM_WI7070", "", "", ["BarCode"], true);

        /// <summary>
        /// MES_AS_WI1011。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1011 = new("MES_AS_WI1011", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1012。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1012 = new("MES_AS_WI1012", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1013。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1013 = new("MES_AS_WI1013", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1014。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1014 = new("MES_AS_WI1014", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1015。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1015 = new("MES_AS_WI1015", "", "", ["strSql"], true);

        /// <summary>
        /// MES_AS_WI1016。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1016 = new("MES_AS_WI1016", "", "", [], true);

        /// <summary>
        /// MES_AS_WI1017。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1017 = new("MES_AS_WI1017", "", "", [], true);

        /// <summary>
        /// MES_AS_WI1018。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1018 = new("MES_AS_WI1018", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI1019。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1019 = new("MES_AS_WI1019", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101A。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101A = new("MES_AS_WI101A", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101A_Check。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101A_Check = new("MES_AS_WI101A_Check", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101B。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101B = new("MES_AS_WI101B", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101C。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101C = new("MES_AS_WI101C", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101D。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101D = new("MES_AS_WI101D", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101E。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101E = new("MES_AS_WI101E", "", "", ["Barcode", "BarcodeType", "Flag", "HostName", "WriteTime", "Note"], true);

        /// <summary>
        /// MES_AS_WI101F。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101F = new("MES_AS_WI101F", "", "", ["BatterySN"], true);

        /// <summary>
        /// MES_AS_WI101F_1。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101F_1 = new("MES_AS_WI101F_1", "", "", ["BatterySN"], true);

        /// <summary>
        /// MES_AS_WI101G。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101G = new("MES_AS_WI101G", "", "", ["device_id", "yali_shangxian", "yali_xiaxian", "gunya_cishu", "gunya_sudu", "gunya_qishiweizhi", "qiangun_weizhi", "hougun_weizhi", "jiaozhun_yalizhi", "sn", "gunya_maxyali", "gunya_minyali", "pass_fail"], true);

        /// <summary>
        /// MES_AS_WI101K。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101K = new("MES_AS_WI101K", "", "", [], false);

        /// <summary>
        /// MES_AS_WI101KD。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101KD = new("MES_AS_WI101KD", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101L。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101L = new("MES_AS_WI101L", "", "", ["FicSN"], true);

        /// <summary>
        /// MES_AS_WI101S。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101S = new("MES_AS_WI101S", "", "", ["content", "testresult", "psn", "workordernumber", "testStationId", "testStationName", "linename", "hostname", "testTime", "testModel", "testWorkID", "Fixture", "FixtureSN", "Powersupply", "PowersupplySN", "Instrument", "InstrumentSN", "Operator", "testVersion", "keypartlist", "DTitle", "DContent", "state", "errcode", "errmessage"], false);

        /// <summary>
        /// MES_AS_WI101Lock。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101Lock = new("MES_AS_WI101Lock", "", "", [], true);

        /// <summary>
        /// MES_AS_WI101Mes_Lock。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101Mes_Lock = new("MES_AS_WI101Mes_Lock", "", "", [], true);

        /// <summary>
        /// MES_AS_WI101_BatteryCheck。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101_BatteryCheck = new("MES_AS_WI101_BatteryCheck", "", "", ["BatterySN", "Flag", "MO"], true);

        /// <summary>
        /// MES_AS_WI101_BatteryCheck_Model。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101_BatteryCheck_Model = new("MES_AS_WI101_BatteryCheck_Model", "", "", [], true);

        /// <summary>
        /// MES_AS_WI101_HONORTsnCheck。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI101_HONORTsnCheck = new("MES_AS_WI101_HONORTsnCheck", "", "", ["BarCode", "Mo"], true);

        /// <summary>
        /// MES_AS_WI1021。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1021 = new("MES_AS_WI1021", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1022。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1022 = new("MES_AS_WI1022", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1023。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1023 = new("MES_AS_WI1023", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1024_NEWTON。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1024_NEWTON = new("MES_AS_WI1024_NEWTON", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1025。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1025 = new("MES_AS_WI1025", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1026。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1026 = new("MES_AS_WI1026", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1027。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1027 = new("MES_AS_WI1027", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1028。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1028 = new("MES_AS_WI1028", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1029。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1029 = new("MES_AS_WI1029", "", "", [], false);

        /// <summary>
        /// MES_AS_WI102A。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI102A = new("MES_AS_WI102A", "", "", [], false);

        /// <summary>
        /// MES_AS_WI102B。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI102B = new("MES_AS_WI102B", "", "", [], false);

        /// <summary>
        /// MES_AS_WI102C。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI102C = new("MES_AS_WI102C", "", "", [], false);

        /// <summary>
        /// MES_AS_WI1039。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1039 = new("MES_AS_WI1039", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1011。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1011 = new("MES_OPPO_WI1011", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1012。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1012 = new("MES_OPPO_WI1012", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1013。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1013 = new("MES_OPPO_WI1013", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1014。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1014 = new("MES_OPPO_WI1014", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1015。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1015 = new("MES_OPPO_WI1015", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1016。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1016 = new("MES_OPPO_WI1016", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1018。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1018 = new("MES_OPPO_WI1018", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1018_ADD_CUST_MODEL。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1018_ADD_CUST_MODEL = new("MES_OPPO_WI1018_ADD_CUST_MODEL", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1019。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1019 = new("MES_OPPO_WI1019", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1020。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1020 = new("MES_OPPO_WI1020", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1021。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1021 = new("MES_OPPO_WI1021", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1022。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1022 = new("MES_OPPO_WI1022", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1023。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1023 = new("MES_OPPO_WI1023", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1024。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1024 = new("MES_OPPO_WI1024", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1025。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1025 = new("MES_OPPO_WI1025", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1028。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1028 = new("MES_OPPO_WI1028", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI10201。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI10201 = new("MES_OPPO_WI10201", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1029。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1029 = new("MES_OPPO_WI1029", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1030。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1030 = new("MES_OPPO_WI1030", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1031。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1031 = new("MES_OPPO_WI1031", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1032。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1032 = new("MES_OPPO_WI1032", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1039。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1039 = new("MES_OPPO_WI1039", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1TAC。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1TAC = new("MES_OPPO_WI1TAC", "", "", [], false);

        /// <summary>
        /// MES_OPPO_WI1AsmCOO。
        /// </summary>
        public static readonly LuxWmiMethod MES_OPPO_WI1AsmCOO = new("MES_OPPO_WI1AsmCOO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI201A_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI201A_VIVO = new("MES_PA_WI201A_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI201B_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI201B_VIVO = new("MES_PA_WI201B_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI201C_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI201C_VIVO = new("MES_PA_WI201C_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI201D_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI201D_VIVO = new("MES_PA_WI201D_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI202A_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI202A_VIVO = new("MES_PA_WI202A_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI202B_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI202B_VIVO = new("MES_PA_WI202B_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI202A_CMEI_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI202A_CMEI_VIVO = new("MES_PA_WI202A_CMEI_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI202B_CMEI_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI202B_CMEI_VIVO = new("MES_PA_WI202B_CMEI_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI203A_CMEI_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI203A_CMEI_VIVO = new("MES_PA_WI203A_CMEI_VIVO", "", "", [], false);

        /// <summary>
        /// MES_PA_WI203B_CMEI_VIVO。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI203B_CMEI_VIVO = new("MES_PA_WI203B_CMEI_VIVO", "", "", [], false);

        /// <summary>
        /// SMT_BIG_ERR_MSG。
        /// </summary>
        public static readonly LuxWmiMethod SMT_BIG_ERR_MSG = new("SMT_BIG_ERR_MSG", "", "", [], false);

        /// <summary>
        /// SMT_BIG_ERR_CAUSE。
        /// </summary>
        public static readonly LuxWmiMethod SMT_BIG_ERR_CAUSE = new("SMT_BIG_ERR_CAUSE", "", "", [], false);

        /// <summary>
        /// SMT_ERR_CAUSATION。
        /// </summary>
        public static readonly LuxWmiMethod SMT_ERR_CAUSATION = new("SMT_ERR_CAUSATION", "", "", [], false);

        /// <summary>
        /// SMT_ERR_CAUSE_CODE。
        /// </summary>
        public static readonly LuxWmiMethod SMT_ERR_CAUSE_CODE = new("SMT_ERR_CAUSE_CODE", "", "", [], false);

        /// <summary>
        /// SMT_GET_OLDCSN。
        /// </summary>
        public static readonly LuxWmiMethod SMT_GET_OLDCSN = new("SMT_GET_OLDCSN", "", "", [], false);

        /// <summary>
        /// SAP中的项目配置表。
        /// </summary>
        public static readonly LuxWmiMethod MES_SAP_WIA008 = new("MES_SAP_WIA008", "SAP中的项目配置表", "", [], false);

        /// <summary>
        /// 查询批次号。
        /// </summary>
        public static readonly LuxWmiMethod MES_API_WIA032 = new("MES_API_WIA032", "查询批次号", "", [], false);

        /// <summary>
        /// 根据工单和Key类型申请批次号。
        /// </summary>
        public static readonly LuxWmiMethod MES_API_WIA031 = new("MES_API_WIA031", "根据工单和Key类型申请批次号", "", [], false);

        /// <summary>
        /// 根据批次号查询批次号绑定的KEY。
        /// </summary>
        public static readonly LuxWmiMethod MES_API_WIA033 = new("MES_API_WIA033", "根据批次号查询批次号绑定的KEY", "", [], false);

        /// <summary>
        /// MES_AS_WI1032。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_AS_WI1032 = new("MES_AS_WI1032", "", "仅 5.0.3", ["PSN", "S_NM", "KP_NM", "KP_Value"], false);

        /// <summary>
        /// MES_PA_WI1011。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI1011 = new("MES_PA_WI1011", "", "仅 5.0.3", ["BarCode", "FpOrderNumber"], false);

        /// <summary>
        /// MES_PA_WI1012S。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI1012S = new("MES_PA_WI1012S", "", "仅 5.0.3", ["BarCode"], false);

        /// <summary>
        /// MES_PA_WI1019。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI1019 = new("MES_PA_WI1019", "", "仅 5.0.3", ["strValue", "strType"], false);

        /// <summary>
        /// MES_PA_WI101F。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI101F = new("MES_PA_WI101F", "", "仅 5.0.3", ["BarCode", "BarCodeType"], false);

        /// <summary>
        /// MES_PA_WI101T。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI101T = new("MES_PA_WI101T", "", "仅 5.0.3", ["BarCode", "BarCodeType"], false);

        /// <summary>
        /// MES_PA_WI1034。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI1034 = new("MES_PA_WI1034", "", "仅 5.0.3", ["BarCode", "BarCodeType"], false);

        /// <summary>
        /// MES_PA_WI101I。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod MES_PA_WI101I = new("MES_PA_WI101I", "", "仅 5.0.3", ["BarCode", "BarCodeType"], false);

        /// <summary>
        /// QUERY_RECORD。仅 5.0.3。
        /// </summary>
        public static readonly LuxWmiMethod QUERY_RECORD = new("QUERY_RECORD", "", "仅 5.0.3", ["BarCode", "StationID", "LineNumber", "target", "WorkOrder", "CheckRoute", "SiteProcess", "SiteParameters", "CheckPhyNumber", "KeyPartsLink", "SpecialList"], false);

        /// <summary>
        /// 全部方法，顺序与两份目录合并后的顺序一致。
        /// </summary>
        public static IReadOnlyList<LuxWmiMethod> All { get; } =
        [
            MES_BA_WI6011,
            MES_BA_WI601L,
            MES_BA_WI601K,
            MES_BA_WI601S,
            MES_BA_WI6012,
            MES_BA_WI6013,
            MES_BA_WI6014,
            MES_BA_WI6015,
            MES_BA_WI6016,
            MES_BA_WI6017,
            MES_BA_WI6018,
            MES_BA_WI6019,
            MES_BA_WI601A,
            MES_BA_WI601B,
            MES_BA_WI601C,
            MES_BA_WI601D,
            MES_BA_WI601E,
            MES_BA_WI601E1,
            MES_BA_WI601F,
            MES_BA_WI601G,
            MES_BA_WI601H,
            MES_BA_WI601I,
            MES_BA_WI601M,
            MES_BA_WI601N,
            MES_BA_WI601KD,
            MES_BA_WI601_QC,
            MES_BA_WI601Q,
            MES_BA_WI601R,
            MES_BA_WI601W,
            MES_BA_WI601T,
            MES_BA_WI601U,
            MES_BA_WI601V,
            MES_BA_WI6021,
            MES_BA_WI602L,
            MES_BA_WI6022,
            MES_BA_WI6023,
            MES_BA_WI6023_GetConnection_Internal,
            MES_BA_WI6024,
            MES_BA_WI6024_V2,
            MES_BA_WI6025,
            MES_BA_WI6026,
            MES_BA_WI6026_KD,
            MES_BA_WI6027,
            MES_BA_WI6028,
            MES_BA_WI6029,
            MES_BA_WI602A,
            MES_BA_WI602B,
            MES_BA_WI602C,
            MES_BA_WI602D,
            MES_BA_WI602E,
            MES_BA_WI602ALL,
            MES_BA_WI60210,
            MES_BA_WI60211,
            MES_BA_WI6030,
            MES_BA_WI6031,
            MES_BA_WI6032,
            MES_BA_WI6033,
            MES_BA_WI6034,
            MES_BA_WI6035,
            MES_BA_WI6036,
            MES_BA_WI6041,
            MES_BA_WI6041_EXT_BLL,
            MES_BA_WI6042,
            MES_BA_WI604D,
            MES_BA_WI6043,
            MES_BA_WI6044,
            MES_BA_WI6046,
            MES_BA_WI6048,
            MES_BA_WI604B,
            MES_BA_WI604C,
            MES_BA_WI6049,
            MES_BA_WI604E,
            MES_BA_WI604A,
            MES_BA_WI604F,
            MES_BA_WI604W,
            MES_BA_WI604H,
            MES_BA_WI604BU,
            MES_BA_WI604HW,
            MES_BA_WI604O,
            MES_BA_WI6051,
            MES_BA_WI6052,
            MES_BA_WI6054,
            MES_BA_WI6055,
            MES_BA_WI6061,
            MES_BA_WI6064,
            MES_BA_WI6071,
            MES_BA_WI6072,
            MES_BA_WI6073,
            MES_BA_WI6074,
            MES_BA_WI6075,
            MES_BA_WI6076,
            MES_BA_WI6077,
            MES_BA_WI6078,
            MES_BA_WI6078_LogBefore,
            MES_BA_WI6078_Log,
            MES_BA_WI6078_LogCreateBefore,
            MES_BA_WI6078_LogCreate,
            MES_BA_WI6078_LogAddPcbaBefore,
            MES_BA_WI6078_LogAddPcba,
            MES_BA_WI6078_LogGetPcbaBefore,
            MES_BA_WI6078_LogGetPcba,
            MES_BA_WI6078_SourcPSN,
            MES_BA_WI6078_ASSYBefore,
            MES_BA_WI6078_ASSY,
            MES_BA_WI6078_ByAllWo,
            MES_BA_WI6078_ByWo,
            MES_BA_WI6078_SMTBefore,
            MES_BA_WI6078_SMT,
            MES_BA_WI6078_TSP,
            MES_BA_WI6078_TSPByPCBSN,
            MES_BA_WI6078_SpListBefore,
            MES_BA_WI6078_SpList,
            MES_BA_WI6079,
            MES_BA_WI6081,
            MES_BA_WI6082,
            MES_BA_WI6083,
            MES_BA_WI6084,
            MES_BA_WI6085,
            MES_SM_WI7011,
            MES_SM_WI7012,
            MES_SM_WI7013,
            MES_SM_WI7014,
            MES_SM_WI7015,
            MES_SM_WI7016,
            MES_SM_WI7017,
            MES_SM_WI7022,
            MES_SM_WI7023,
            MES_SM_WI7024,
            MES_SM_WI702A,
            MES_SM_WI7031,
            MES_SM_WI7032,
            MES_SM_WI7021,
            MES_SM_WI7041,
            MES_SM_WI7042,
            MES_SM_WI7043,
            MES_SM_WI7044,
            MES_SM_WI7045,
            MES_SM_WI7046,
            MES_SM_WI7047,
            MES_SM_WI7048,
            MES_SM_WI7049,
            MES_SM_WI7051,
            MES_SM_WI7052,
            MES_SM_WI7053,
            MES_SM_WI7054,
            MES_SM_WI7055,
            MES_SM_WI7056,
            MES_SM_WI7057,
            MES_SM_WI7058,
            MES_SM_WI7059,
            MES_SM_WI705A,
            MES_SM_WI705B,
            MES_SM_WI7060,
            MES_SM_WI706A,
            MES_SM_WI706B,
            MES_SM_WI706C,
            MES_SM_WI7070,
            MES_AS_WI1011,
            MES_AS_WI1012,
            MES_AS_WI1013,
            MES_AS_WI1014,
            MES_AS_WI1015,
            MES_AS_WI1016,
            MES_AS_WI1017,
            MES_AS_WI1018,
            MES_AS_WI1019,
            MES_AS_WI101A,
            MES_AS_WI101A_Check,
            MES_AS_WI101B,
            MES_AS_WI101C,
            MES_AS_WI101D,
            MES_AS_WI101E,
            MES_AS_WI101F,
            MES_AS_WI101F_1,
            MES_AS_WI101G,
            MES_AS_WI101K,
            MES_AS_WI101KD,
            MES_AS_WI101L,
            MES_AS_WI101S,
            MES_AS_WI101Lock,
            MES_AS_WI101Mes_Lock,
            MES_AS_WI101_BatteryCheck,
            MES_AS_WI101_BatteryCheck_Model,
            MES_AS_WI101_HONORTsnCheck,
            MES_AS_WI1021,
            MES_AS_WI1022,
            MES_AS_WI1023,
            MES_AS_WI1024_NEWTON,
            MES_AS_WI1025,
            MES_AS_WI1026,
            MES_AS_WI1027,
            MES_AS_WI1028,
            MES_AS_WI1029,
            MES_AS_WI102A,
            MES_AS_WI102B,
            MES_AS_WI102C,
            MES_AS_WI1039,
            MES_OPPO_WI1011,
            MES_OPPO_WI1012,
            MES_OPPO_WI1013,
            MES_OPPO_WI1014,
            MES_OPPO_WI1015,
            MES_OPPO_WI1016,
            MES_OPPO_WI1018,
            MES_OPPO_WI1018_ADD_CUST_MODEL,
            MES_OPPO_WI1019,
            MES_OPPO_WI1020,
            MES_OPPO_WI1021,
            MES_OPPO_WI1022,
            MES_OPPO_WI1023,
            MES_OPPO_WI1024,
            MES_OPPO_WI1025,
            MES_OPPO_WI1028,
            MES_OPPO_WI10201,
            MES_OPPO_WI1029,
            MES_OPPO_WI1030,
            MES_OPPO_WI1031,
            MES_OPPO_WI1032,
            MES_OPPO_WI1039,
            MES_OPPO_WI1TAC,
            MES_OPPO_WI1AsmCOO,
            MES_PA_WI201A_VIVO,
            MES_PA_WI201B_VIVO,
            MES_PA_WI201C_VIVO,
            MES_PA_WI201D_VIVO,
            MES_PA_WI202A_VIVO,
            MES_PA_WI202B_VIVO,
            MES_PA_WI202A_CMEI_VIVO,
            MES_PA_WI202B_CMEI_VIVO,
            MES_PA_WI203A_CMEI_VIVO,
            MES_PA_WI203B_CMEI_VIVO,
            SMT_BIG_ERR_MSG,
            SMT_BIG_ERR_CAUSE,
            SMT_ERR_CAUSATION,
            SMT_ERR_CAUSE_CODE,
            SMT_GET_OLDCSN,
            MES_SAP_WIA008,
            MES_API_WIA032,
            MES_API_WIA031,
            MES_API_WIA033,
            MES_AS_WI1032,
            MES_PA_WI1011,
            MES_PA_WI1012S,
            MES_PA_WI1019,
            MES_PA_WI101F,
            MES_PA_WI101T,
            MES_PA_WI1034,
            MES_PA_WI101I,
            QUERY_RECORD,
        ];
    }
}
