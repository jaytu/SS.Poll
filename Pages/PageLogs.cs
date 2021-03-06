﻿using System;
using System.Web;
using System.Web.UI.WebControls;
using SS.Poll.Core;
using System.Collections.Generic;
using SS.Poll.Models;
using SS.Poll.Provider;

namespace SS.Poll.Pages
{
    public class PageLogs : PageBase
    {
        public Literal LtlMessage;
        public Literal LtlFieldNames;
        public Repeater RptLogs;
        public Button BtnExport;

        public Literal LtlScript;

        private List<FieldInfo> _fieldInfoList;

        public static string GetRedirectUrl(int siteId, int channelId, int contentId, string returnUrl)
        {
            return $"{nameof(PageLogs)}.aspx?siteId={siteId}&channelId={channelId}&contentId={contentId}&returnUrl={HttpUtility.UrlEncode(returnUrl)}";
        }

        public void Page_Load(object sender, EventArgs e)
        {
            _fieldInfoList = FieldDao.GetFieldInfoList(SiteId, ChannelId, ContentId, false);

            if (IsPostBack) return;

            foreach (var fieldInfo in _fieldInfoList)
            {
                LtlFieldNames.Text += $@"<th scope=""col"">{fieldInfo.DisplayName}</th>";
            }

            var totalCount = LogDao.GetCount(SiteId, ChannelId, ContentId);
            var logs = LogDao.GetPollLogInfoList(SiteId, ChannelId, ContentId, totalCount, 30, 0);

            RptLogs.DataSource = logs;
            RptLogs.ItemDataBound += RptLogs_ItemDataBound;
            RptLogs.DataBind();
        }

        public void BtnExport_Click(object sender, EventArgs e)
        {
            var logs = LogDao.GetAllPollLogInfoList(SiteId, ChannelId, ContentId);

            var head = new List<string> { "序号"};
            foreach (var fieldInfo in _fieldInfoList)
            {
                head.Add(fieldInfo.DisplayName);
            }
            head.Add("提交时间");

            var rows = new List<List<string>>();

            var index = 1;
            foreach (var log in logs)
            {
                var row = new List<string>
                {
                    index++.ToString()
                };
                foreach (var fieldInfo in _fieldInfoList)
                {
                    row.Add(log.GetString(fieldInfo.AttributeName));
                }
                row.Add(log.AddDate.ToString("yyyy-MM-dd HH:mm"));

                rows.Add(row);
            }

            var relatedPath = "投票提交记录.csv";

            CsvUtils.Export(SiteServer.Plugin.Context.PluginApi.GetPluginPath(Main.PluginId, relatedPath), head, rows);

            HttpContext.Current.Response.Redirect(SiteServer.Plugin.Context.PluginApi.GetPluginUrl(Main.PluginId, relatedPath));
        }

        private void RptLogs_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var logInfo = (LogInfo)e.Item.DataItem;

            var ltlValues = (Literal)e.Item.FindControl("ltlValues");
            var ltlAddDate = (Literal)e.Item.FindControl("ltlAddDate");

            foreach (var fieldInfo in _fieldInfoList)
            {
                ltlValues.Text += $@"<td>{logInfo.GetString(fieldInfo.AttributeName)}</td>";
            }

            ltlAddDate.Text = logInfo.AddDate.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
