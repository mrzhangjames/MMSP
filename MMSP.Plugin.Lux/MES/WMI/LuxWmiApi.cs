namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// 每个目录方法一个入口。参数用文档字段名组成的字典，缺省字段不代填。
    /// </summary>
    public static class LuxWmiApi
    {
        /// <summary>
        /// 检查是否允许过站。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6011(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6011, parameters);

        /// <summary>
        /// 检查是否允许过站--针对点胶、预加工。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601L(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601L, parameters);

        /// <summary>
        /// 检查是否允许过站针对笔电混投。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601K(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601K, parameters);

        /// <summary>
        /// 检查是否允许过站针对服务器项目。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601S(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601S, parameters);

        /// <summary>
        /// 各站点过站处理,已经取消处理。已取消。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6012(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6012, parameters);

        /// <summary>
        /// 验证账号密码是否正确。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6013(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6013, parameters);

        /// <summary>
        /// 验证账号是否有对应模块的权限。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6014(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6014, parameters);

        /// <summary>
        /// MES_BA_WI6015。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6015(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6015, parameters);

        /// <summary>
        /// MES_BA_WI6016。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6016(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6016, parameters);

        /// <summary>
        /// 获取工厂。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6017(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6017, parameters);

        /// <summary>
        /// MES_BA_WI6018。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6018(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6018, parameters);

        /// <summary>
        /// MES_BA_WI6019。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6019(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6019, parameters);

        /// <summary>
        /// 已经取消处理。已取消。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601A, parameters);

        /// <summary>
        /// MES_BA_WI601B。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601B, parameters);

        /// <summary>
        /// MES_BA_WI601C。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601C(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601C, parameters);

        /// <summary>
        /// MES_BA_WI601D。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601D(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601D, parameters);

        /// <summary>
        /// MES_BA_WI601E。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601E(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601E, parameters);

        /// <summary>
        /// 目前未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601E1(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601E1, parameters);

        /// <summary>
        /// MES_BA_WI601F。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601F(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601F, parameters);

        /// <summary>
        /// MES_BA_WI601G。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601G(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601G, parameters);

        /// <summary>
        /// MES_BA_WI601H。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601H(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601H, parameters);

        /// <summary>
        /// 目前已停产。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601I(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601I, parameters);

        /// <summary>
        /// MES_BA_WI601M。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601M(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601M, parameters);

        /// <summary>
        /// 目前已停产。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601N(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601N, parameters);

        /// <summary>
        /// MES_BA_WI601KD。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601KD(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601KD, parameters);

        /// <summary>
        /// MES_BA_WI601_QC。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601_QC(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601_QC, parameters);

        /// <summary>
        /// MES_BA_WI601Q。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601Q(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601Q, parameters);

        /// <summary>
        /// MES_BA_WI601R。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601R(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601R, parameters);

        /// <summary>
        /// MES_BA_WI601W。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601W(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601W, parameters);

        /// <summary>
        /// MES_BA_WI601T。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601T(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601T, parameters);

        /// <summary>
        /// MES_BA_WI601U。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601U(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601U, parameters);

        /// <summary>
        /// MES_BA_WI601V。
        /// </summary>
        public static LuxWmiResult MES_BA_WI601V(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI601V, parameters);

        /// <summary>
        /// MES_BA_WI6021。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6021(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6021, parameters);

        /// <summary>
        /// MES_BA_WI602L。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602L(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602L, parameters);

        /// <summary>
        /// MES_BA_WI6022。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6022(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6022, parameters);

        /// <summary>
        /// MES_BA_WI6023。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6023(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6023, parameters);

        /// <summary>
        /// MES_BA_WI6023_GetConnection_Internal。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6023_GetConnection_Internal(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6023_GetConnection_Internal, parameters);

        /// <summary>
        /// MES_BA_WI6024。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6024(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6024, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6024_V2(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6024_V2, parameters);

        /// <summary>
        /// MES_BA_WI6025。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6025(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6025, parameters);

        /// <summary>
        /// MES_BA_WI6026。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6026(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6026, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6026_KD(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6026_KD, parameters);

        /// <summary>
        /// MES_BA_WI6027。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6027(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6027, parameters);

        /// <summary>
        /// MES_BA_WI6028。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6028(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6028, parameters);

        /// <summary>
        /// MES_BA_WI6029。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6029(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6029, parameters);

        /// <summary>
        /// MES_BA_WI602A。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602A, parameters);

        /// <summary>
        /// MES_BA_WI602B。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602B, parameters);

        /// <summary>
        /// MES_BA_WI602C。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602C(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602C, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602D(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602D, parameters);

        /// <summary>
        /// MES_BA_WI602E。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602E(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602E, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI602ALL(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI602ALL, parameters);

        /// <summary>
        /// MES_BA_WI60210。
        /// </summary>
        public static LuxWmiResult MES_BA_WI60210(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI60210, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI60211(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI60211, parameters);

        /// <summary>
        /// MES_BA_WI6030。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6030(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6030, parameters);

        /// <summary>
        /// MES_BA_WI6031。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6031(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6031, parameters);

        /// <summary>
        /// MES_BA_WI6032。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6032(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6032, parameters);

        /// <summary>
        /// MES_BA_WI6033。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6033(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6033, parameters);

        /// <summary>
        /// MES_BA_WI6034。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6034(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6034, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6035(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6035, parameters);

        /// <summary>
        /// MES_BA_WI6036。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6036(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6036, parameters);

        /// <summary>
        /// MES_BA_WI6041。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6041(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6041, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6041_EXT_BLL(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6041_EXT_BLL, parameters);

        /// <summary>
        /// MES_BA_WI6042。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6042(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6042, parameters);

        /// <summary>
        /// MES_BA_WI604D。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604D(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604D, parameters);

        /// <summary>
        /// MES_BA_WI6043。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6043(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6043, parameters);

        /// <summary>
        /// MES_BA_WI6044。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6044(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6044, parameters);

        /// <summary>
        /// MES_BA_WI6046。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6046(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6046, parameters);

        /// <summary>
        /// MES_BA_WI6048。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6048(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6048, parameters);

        /// <summary>
        /// MES_BA_WI604B。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604B, parameters);

        /// <summary>
        /// MES_BA_WI604C。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604C(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604C, parameters);

        /// <summary>
        /// MES_BA_WI6049。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6049(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6049, parameters);

        /// <summary>
        /// MES_BA_WI604E。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604E(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604E, parameters);

        /// <summary>
        /// MES_BA_WI604A。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604A, parameters);

        /// <summary>
        /// MES_BA_WI604F。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604F(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604F, parameters);

        /// <summary>
        /// MES_BA_WI604W。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604W(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604W, parameters);

        /// <summary>
        /// MES_BA_WI604H。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604H(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604H, parameters);

        /// <summary>
        /// MES_BA_WI604BU。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604BU(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604BU, parameters);

        /// <summary>
        /// MES_BA_WI604HW。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604HW(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604HW, parameters);

        /// <summary>
        /// MES_BA_WI604O。
        /// </summary>
        public static LuxWmiResult MES_BA_WI604O(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI604O, parameters);

        /// <summary>
        /// MES_BA_WI6051。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6051(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6051, parameters);

        /// <summary>
        /// 孟加拉，国内不用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6052(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6052, parameters);

        /// <summary>
        /// HWPAD返回维修表的是否有input状态的记录。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6054(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6054, parameters);

        /// <summary>
        /// HWPAD检查CSN发料是否在工单的BOM表中。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6055(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6055, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6061(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6061, parameters);

        /// <summary>
        /// MES_BA_WI6064。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6064(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6064, parameters);

        /// <summary>
        /// MES_BA_WI6071。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6071(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6071, parameters);

        /// <summary>
        /// MES_BA_WI6072。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6072(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6072, parameters);

        /// <summary>
        /// MES_BA_WI6073。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6073(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6073, parameters);

        /// <summary>
        /// MES_BA_WI6074。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6074(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6074, parameters);

        /// <summary>
        /// MES_BA_WI6075。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6075(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6075, parameters);

        /// <summary>
        /// MES_BA_WI6076。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6076(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6076, parameters);

        /// <summary>
        /// MES_BA_WI6077。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6077(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6077, parameters);

        /// <summary>
        /// MES_BA_WI6078。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_Log。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_Log(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_Log, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogCreateBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogCreateBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogCreateBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogCreate。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogCreate(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogCreate, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogAddPcbaBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogAddPcbaBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogAddPcbaBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogAddPcba。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogAddPcba(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogAddPcba, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogGetPcbaBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogGetPcbaBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogGetPcbaBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_LogGetPcba。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_LogGetPcba(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_LogGetPcba, parameters);

        /// <summary>
        /// MES_BA_WI6078_SourcPSN。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_SourcPSN(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_SourcPSN, parameters);

        /// <summary>
        /// MES_BA_WI6078_ASSYBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_ASSYBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_ASSYBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_ASSY。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_ASSY(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_ASSY, parameters);

        /// <summary>
        /// MES_BA_WI6078_ByAllWo。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_ByAllWo(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_ByAllWo, parameters);

        /// <summary>
        /// MES_BA_WI6078_ByWo。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_ByWo(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_ByWo, parameters);

        /// <summary>
        /// MES_BA_WI6078_SMTBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_SMTBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_SMTBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_SMT。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_SMT(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_SMT, parameters);

        /// <summary>
        /// MES_BA_WI6078_TSP。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_TSP(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_TSP, parameters);

        /// <summary>
        /// MES_BA_WI6078_TSPByPCBSN。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_TSPByPCBSN(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_TSPByPCBSN, parameters);

        /// <summary>
        /// MES_BA_WI6078_SpListBefore。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_SpListBefore(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_SpListBefore, parameters);

        /// <summary>
        /// MES_BA_WI6078_SpList。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6078_SpList(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6078_SpList, parameters);

        /// <summary>
        /// MES_BA_WI6079。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6079(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6079, parameters);

        /// <summary>
        /// 未启用。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6081(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6081, parameters);

        /// <summary>
        /// MES_BA_WI6082。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6082(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6082, parameters);

        /// <summary>
        /// MES_BA_WI6083。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6083(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6083, parameters);

        /// <summary>
        /// MES_BA_WI6084。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6084(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6084, parameters);

        /// <summary>
        /// MES_BA_WI6085。
        /// </summary>
        public static LuxWmiResult MES_BA_WI6085(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_BA_WI6085, parameters);

        /// <summary>
        /// MES_SM_WI7011。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7011(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7011, parameters);

        /// <summary>
        /// MES_SM_WI7012。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7012(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7012, parameters);

        /// <summary>
        /// MES_SM_WI7013。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7013(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7013, parameters);

        /// <summary>
        /// MES_SM_WI7014。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7014(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7014, parameters);

        /// <summary>
        /// MES_SM_WI7015。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7015(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7015, parameters);

        /// <summary>
        /// MES_SM_WI7016。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7016(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7016, parameters);

        /// <summary>
        /// MES_SM_WI7017。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7017(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7017, parameters);

        /// <summary>
        /// MES_SM_WI7022。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7022(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7022, parameters);

        /// <summary>
        /// MES_SM_WI7023。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7023(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7023, parameters);

        /// <summary>
        /// MES_SM_WI7024。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7024(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7024, parameters);

        /// <summary>
        /// MES_SM_WI702A。
        /// </summary>
        public static LuxWmiResult MES_SM_WI702A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI702A, parameters);

        /// <summary>
        /// MES_SM_WI7031。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7031(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7031, parameters);

        /// <summary>
        /// MES_SM_WI7032。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7032(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7032, parameters);

        /// <summary>
        /// MES_SM_WI7021。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7021(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7021, parameters);

        /// <summary>
        /// MES_SM_WI7041。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7041(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7041, parameters);

        /// <summary>
        /// MES_SM_WI7042。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7042(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7042, parameters);

        /// <summary>
        /// MES_SM_WI7043。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7043(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7043, parameters);

        /// <summary>
        /// MES_SM_WI7044。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7044(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7044, parameters);

        /// <summary>
        /// MES_SM_WI7045。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7045(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7045, parameters);

        /// <summary>
        /// MES_SM_WI7046。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7046(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7046, parameters);

        /// <summary>
        /// MES_SM_WI7047。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7047(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7047, parameters);

        /// <summary>
        /// MES_SM_WI7048。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7048(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7048, parameters);

        /// <summary>
        /// MES_SM_WI7049。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7049(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7049, parameters);

        /// <summary>
        /// MES_SM_WI7051。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7051(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7051, parameters);

        /// <summary>
        /// MES_SM_WI7052。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7052(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7052, parameters);

        /// <summary>
        /// MES_SM_WI7053。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7053(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7053, parameters);

        /// <summary>
        /// MES_SM_WI7054。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7054(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7054, parameters);

        /// <summary>
        /// MES_SM_WI7055。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7055(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7055, parameters);

        /// <summary>
        /// MES_SM_WI7056。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7056(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7056, parameters);

        /// <summary>
        /// MES_SM_WI7057。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7057(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7057, parameters);

        /// <summary>
        /// MES_SM_WI7058。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7058(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7058, parameters);

        /// <summary>
        /// MES_SM_WI7059。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7059(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7059, parameters);

        /// <summary>
        /// MES_SM_WI705A。
        /// </summary>
        public static LuxWmiResult MES_SM_WI705A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI705A, parameters);

        /// <summary>
        /// MES_SM_WI705B。
        /// </summary>
        public static LuxWmiResult MES_SM_WI705B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI705B, parameters);

        /// <summary>
        /// MES_SM_WI7060。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7060(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7060, parameters);

        /// <summary>
        /// MES_SM_WI706A。
        /// </summary>
        public static LuxWmiResult MES_SM_WI706A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI706A, parameters);

        /// <summary>
        /// MES_SM_WI706B。
        /// </summary>
        public static LuxWmiResult MES_SM_WI706B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI706B, parameters);

        /// <summary>
        /// MES_SM_WI706C。
        /// </summary>
        public static LuxWmiResult MES_SM_WI706C(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI706C, parameters);

        /// <summary>
        /// MES_SM_WI7070。
        /// </summary>
        public static LuxWmiResult MES_SM_WI7070(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SM_WI7070, parameters);

        /// <summary>
        /// MES_AS_WI1011。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1011(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1011, parameters);

        /// <summary>
        /// MES_AS_WI1012。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1012(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1012, parameters);

        /// <summary>
        /// MES_AS_WI1013。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1013(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1013, parameters);

        /// <summary>
        /// MES_AS_WI1014。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1014(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1014, parameters);

        /// <summary>
        /// MES_AS_WI1015。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1015(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1015, parameters);

        /// <summary>
        /// MES_AS_WI1016。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1016(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1016, parameters);

        /// <summary>
        /// MES_AS_WI1017。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1017(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1017, parameters);

        /// <summary>
        /// MES_AS_WI1018。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1018(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1018, parameters);

        /// <summary>
        /// MES_AS_WI1019。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1019(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1019, parameters);

        /// <summary>
        /// MES_AS_WI101A。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101A, parameters);

        /// <summary>
        /// MES_AS_WI101A_Check。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101A_Check(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101A_Check, parameters);

        /// <summary>
        /// MES_AS_WI101B。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101B, parameters);

        /// <summary>
        /// MES_AS_WI101C。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101C(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101C, parameters);

        /// <summary>
        /// MES_AS_WI101D。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101D(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101D, parameters);

        /// <summary>
        /// MES_AS_WI101E。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101E(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101E, parameters);

        /// <summary>
        /// MES_AS_WI101F。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101F(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101F, parameters);

        /// <summary>
        /// MES_AS_WI101F_1。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101F_1(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101F_1, parameters);

        /// <summary>
        /// MES_AS_WI101G。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101G(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101G, parameters);

        /// <summary>
        /// MES_AS_WI101K。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101K(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101K, parameters);

        /// <summary>
        /// MES_AS_WI101KD。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101KD(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101KD, parameters);

        /// <summary>
        /// MES_AS_WI101L。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101L(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101L, parameters);

        /// <summary>
        /// MES_AS_WI101S。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101S(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101S, parameters);

        /// <summary>
        /// MES_AS_WI101Lock。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101Lock(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101Lock, parameters);

        /// <summary>
        /// MES_AS_WI101Mes_Lock。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101Mes_Lock(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101Mes_Lock, parameters);

        /// <summary>
        /// MES_AS_WI101_BatteryCheck。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101_BatteryCheck(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101_BatteryCheck, parameters);

        /// <summary>
        /// MES_AS_WI101_BatteryCheck_Model。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101_BatteryCheck_Model(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101_BatteryCheck_Model, parameters);

        /// <summary>
        /// MES_AS_WI101_HONORTsnCheck。
        /// </summary>
        public static LuxWmiResult MES_AS_WI101_HONORTsnCheck(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI101_HONORTsnCheck, parameters);

        /// <summary>
        /// MES_AS_WI1021。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1021(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1021, parameters);

        /// <summary>
        /// MES_AS_WI1022。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1022(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1022, parameters);

        /// <summary>
        /// MES_AS_WI1023。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1023(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1023, parameters);

        /// <summary>
        /// MES_AS_WI1024_NEWTON。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1024_NEWTON(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1024_NEWTON, parameters);

        /// <summary>
        /// MES_AS_WI1025。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1025(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1025, parameters);

        /// <summary>
        /// MES_AS_WI1026。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1026(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1026, parameters);

        /// <summary>
        /// MES_AS_WI1027。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1027(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1027, parameters);

        /// <summary>
        /// MES_AS_WI1028。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1028(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1028, parameters);

        /// <summary>
        /// MES_AS_WI1029。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1029(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1029, parameters);

        /// <summary>
        /// MES_AS_WI102A。
        /// </summary>
        public static LuxWmiResult MES_AS_WI102A(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI102A, parameters);

        /// <summary>
        /// MES_AS_WI102B。
        /// </summary>
        public static LuxWmiResult MES_AS_WI102B(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI102B, parameters);

        /// <summary>
        /// MES_AS_WI102C。
        /// </summary>
        public static LuxWmiResult MES_AS_WI102C(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI102C, parameters);

        /// <summary>
        /// MES_AS_WI1039。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1039(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1039, parameters);

        /// <summary>
        /// MES_OPPO_WI1011。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1011(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1011, parameters);

        /// <summary>
        /// MES_OPPO_WI1012。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1012(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1012, parameters);

        /// <summary>
        /// MES_OPPO_WI1013。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1013(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1013, parameters);

        /// <summary>
        /// MES_OPPO_WI1014。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1014(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1014, parameters);

        /// <summary>
        /// MES_OPPO_WI1015。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1015(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1015, parameters);

        /// <summary>
        /// MES_OPPO_WI1016。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1016(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1016, parameters);

        /// <summary>
        /// MES_OPPO_WI1018。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1018(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1018, parameters);

        /// <summary>
        /// MES_OPPO_WI1018_ADD_CUST_MODEL。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1018_ADD_CUST_MODEL(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1018_ADD_CUST_MODEL, parameters);

        /// <summary>
        /// MES_OPPO_WI1019。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1019(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1019, parameters);

        /// <summary>
        /// MES_OPPO_WI1020。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1020(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1020, parameters);

        /// <summary>
        /// MES_OPPO_WI1021。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1021(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1021, parameters);

        /// <summary>
        /// MES_OPPO_WI1022。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1022(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1022, parameters);

        /// <summary>
        /// MES_OPPO_WI1023。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1023(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1023, parameters);

        /// <summary>
        /// MES_OPPO_WI1024。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1024(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1024, parameters);

        /// <summary>
        /// MES_OPPO_WI1025。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1025(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1025, parameters);

        /// <summary>
        /// MES_OPPO_WI1028。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1028(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1028, parameters);

        /// <summary>
        /// MES_OPPO_WI10201。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI10201(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI10201, parameters);

        /// <summary>
        /// MES_OPPO_WI1029。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1029(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1029, parameters);

        /// <summary>
        /// MES_OPPO_WI1030。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1030(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1030, parameters);

        /// <summary>
        /// MES_OPPO_WI1031。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1031(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1031, parameters);

        /// <summary>
        /// MES_OPPO_WI1032。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1032(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1032, parameters);

        /// <summary>
        /// MES_OPPO_WI1039。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1039(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1039, parameters);

        /// <summary>
        /// MES_OPPO_WI1TAC。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1TAC(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1TAC, parameters);

        /// <summary>
        /// MES_OPPO_WI1AsmCOO。
        /// </summary>
        public static LuxWmiResult MES_OPPO_WI1AsmCOO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_OPPO_WI1AsmCOO, parameters);

        /// <summary>
        /// MES_PA_WI201A_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI201A_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI201A_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI201B_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI201B_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI201B_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI201C_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI201C_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI201C_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI201D_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI201D_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI201D_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI202A_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI202A_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI202A_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI202B_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI202B_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI202B_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI202A_CMEI_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI202A_CMEI_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI202A_CMEI_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI202B_CMEI_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI202B_CMEI_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI202B_CMEI_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI203A_CMEI_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI203A_CMEI_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI203A_CMEI_VIVO, parameters);

        /// <summary>
        /// MES_PA_WI203B_CMEI_VIVO。
        /// </summary>
        public static LuxWmiResult MES_PA_WI203B_CMEI_VIVO(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI203B_CMEI_VIVO, parameters);

        /// <summary>
        /// SMT_BIG_ERR_MSG。
        /// </summary>
        public static LuxWmiResult SMT_BIG_ERR_MSG(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.SMT_BIG_ERR_MSG, parameters);

        /// <summary>
        /// SMT_BIG_ERR_CAUSE。
        /// </summary>
        public static LuxWmiResult SMT_BIG_ERR_CAUSE(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.SMT_BIG_ERR_CAUSE, parameters);

        /// <summary>
        /// SMT_ERR_CAUSATION。
        /// </summary>
        public static LuxWmiResult SMT_ERR_CAUSATION(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.SMT_ERR_CAUSATION, parameters);

        /// <summary>
        /// SMT_ERR_CAUSE_CODE。
        /// </summary>
        public static LuxWmiResult SMT_ERR_CAUSE_CODE(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.SMT_ERR_CAUSE_CODE, parameters);

        /// <summary>
        /// SMT_GET_OLDCSN。
        /// </summary>
        public static LuxWmiResult SMT_GET_OLDCSN(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.SMT_GET_OLDCSN, parameters);

        /// <summary>
        /// SAP中的项目配置表。
        /// </summary>
        public static LuxWmiResult MES_SAP_WIA008(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_SAP_WIA008, parameters);

        /// <summary>
        /// 查询批次号。
        /// </summary>
        public static LuxWmiResult MES_API_WIA032(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_API_WIA032, parameters);

        /// <summary>
        /// 根据工单和Key类型申请批次号。
        /// </summary>
        public static LuxWmiResult MES_API_WIA031(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_API_WIA031, parameters);

        /// <summary>
        /// 根据批次号查询批次号绑定的KEY。
        /// </summary>
        public static LuxWmiResult MES_API_WIA033(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_API_WIA033, parameters);

        /// <summary>
        /// MES_AS_WI1032。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_AS_WI1032(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_AS_WI1032, parameters);

        /// <summary>
        /// MES_PA_WI1011。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI1011(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI1011, parameters);

        /// <summary>
        /// MES_PA_WI1012S。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI1012S(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI1012S, parameters);

        /// <summary>
        /// MES_PA_WI1019。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI1019(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI1019, parameters);

        /// <summary>
        /// MES_PA_WI101F。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI101F(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI101F, parameters);

        /// <summary>
        /// MES_PA_WI101T。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI101T(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI101T, parameters);

        /// <summary>
        /// MES_PA_WI1034。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI1034(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI1034, parameters);

        /// <summary>
        /// MES_PA_WI101I。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult MES_PA_WI101I(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.MES_PA_WI101I, parameters);

        /// <summary>
        /// QUERY_RECORD。仅 5.0.3。
        /// </summary>
        public static LuxWmiResult QUERY_RECORD(LuxMesWmiConfig config, IReadOnlyDictionary<string, object?>? parameters = null)
            => LuxWmiClient.Call(config, LuxWmiCatalog.QUERY_RECORD, parameters);

    }
}
