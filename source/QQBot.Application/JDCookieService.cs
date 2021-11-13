using Microsoft.EntityFrameworkCore;
using QQBot.DB;
using QQBot.Entities.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QQBot.Application
{
    public class JDCookieService
    {
        QQBotDbContext DbContext;
        QLHttpService qLHttpHelper;
        BaseRepository<QLPanelCookie> QLPanelCookieRepository;
        BaseRepository<JDCookie> JDCookieRepository;
        public JDCookieService(QLHttpService qLHttpHelper)
        {
            DbContext = QQBotDbContext.Instance;
            QLPanelCookieRepository = BaseRepository<QLPanelCookie>.Instance;
            JDCookieRepository = BaseRepository<JDCookie>.Instance;
            this.qLHttpHelper = qLHttpHelper;
        }

        public List<JDCookie> GetJDCookies(string key = null, string qlId = null, bool? available = null)
        {
            var querys = DbContext.JDCookies.AsNoTracking().ToList();

            if (available.HasValue)
            {
                querys = querys.Where(n => n.Available == available.Value).ToList();
            }

            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToLower();
                querys = querys.Where(n => n.PTKey.ToLower().Contains(key) || n.PTPin.ToLower().Contains(key) || n.QQ.ToString().Contains(key)
                || (!string.IsNullOrEmpty(n.Remark) && n.Remark.ToLower().Contains(key)) || (!string.IsNullOrEmpty(n.nickname) && n.nickname.ToLower().Contains(key))).ToList();
            }

            var panelCookies = DbContext.QLPanelCookies.AsNoTracking().ToList();

            var panels = DbContext.QLConfigs.AsNoTracking().ToList();

            #region 删除容器不存在的容器cookie关系数据

            var noPanel = panelCookies.Where(n => !panels.Select(i => i.Id).Contains(n.QLPanelId));
            if (noPanel.Any())
            {
                QLPanelCookieRepository.DeleteRange(noPanel);
            }

            #endregion

            panelCookies = panelCookies.Where(n => panels.Select(i => i.Id).Contains(n.QLPanelId)).ToList();

            foreach (var pc in panelCookies)
            {
                pc.QLPanelName = panels.SingleOrDefault(n => n.Id == pc.QLPanelId)?.Name;
            }

            foreach (var item in querys)
            {
                item.QLPanelCookies = panelCookies.Where(n => n.CookieId == item.Id).ToList();
            }

            if (!string.IsNullOrEmpty(qlId))
            {
                querys = querys.Where(n => n.QLPanelCookies.Any(m => m.QLPanelId == qlId)).ToList();
            }

            return querys.OrderByDescending(n => n.Priority).ToList();
        }

        public List<JDCookie> Delete(List<string> ids)
        {
            var cookies = DbContext.JDCookies.AsNoTracking().Where(n => ids.Contains(n.Id)).ToList();
            if (cookies != null && cookies.Any())
            {
                var qpcs = DbContext.QLPanelCookies.AsNoTracking().Where(y => ids.Contains(y.CookieId)).ToList();
                if (qpcs.Any())
                {
                    var qls = DbContext.QLConfigs.AsNoTracking().Where(n => qpcs.Select(n => n.QLPanelId).Contains(n.Id)).ToList();
                    foreach (var ql in qls)
                    {
                        qLHttpHelper.DeleteEnv(ql, qpcs.Where(n => n.QLPanelId == ql.Id).Select(n => n._id));
                    }
                    QLPanelCookieRepository.DeleteRange(qpcs);
                }
                //JDCookieRepository.DeleteByIds(ids);
            }
            return cookies;
        }

        public List<JDCookie> Delete(long qq)
        {
            var jdCookies = DbContext.JDCookies.AsNoTracking().Where(n => n.QQ == qq).ToList();
            Delete(jdCookies.Select(n => n.Id).ToList());
            return jdCookies;
        }
    }
}