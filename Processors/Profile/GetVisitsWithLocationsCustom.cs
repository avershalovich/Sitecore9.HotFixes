// Decompiled with JetBrains decompiler
// Type: Sitecore.Cintel.Reporting.ReportingServerDatasource.Visits.GetVisitsWithLocations
// Assembly: Sitecore.Cintel, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EF95EA69-91B6-42C2-9BDC-CCAB0642E4C2
// Assembly location: D:\Data\Inetpub\SYM2017\HotfixCurrent\bin\Sitecore.Cintel.dll

using Sitecore.Cintel.Reporting;
using Sitecore.Cintel.Reporting.Processors;
using Sitecore.Cintel.Utility;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sitecore.Cintel.Reporting.ReportingServerDatasource.Visits
{
  public class GetVisitsWithLocations : ReportProcessorBase
  {
    public override void Process(ReportProcessorArgs args)
    {
      DataTable tableWithSchema = this.CreateTableWithSchema();
      Guid contactId = args.ReportParameters.ContactId;
      Guid result;
      if (Guid.TryParse(args.ReportParameters.ViewEntityId, out result))
        this.GetTableFromContactXconnect(tableWithSchema, args.ReportParameters.ContactId, new Guid?(result));
      else
        this.GetTableFromContactXconnect(tableWithSchema, contactId, new Guid?());
      args.QueryResult = tableWithSchema;
    }

    private DataTable CreateTableWithSchema()
    {
      DataTable dataTable = new DataTable();
      dataTable.Columns.AddRange(new DataColumn[21]
      {
        new DataColumn("ContactId", typeof (Guid)),
        new DataColumn("_id", typeof (Guid)),
        new DataColumn("ChannelId", typeof (Guid)),
        new DataColumn("StartDateTime", typeof (DateTime)),
        new DataColumn("EndDateTime", typeof (DateTime)),
        new DataColumn("AspNetSessionId", typeof (Guid)),
        new DataColumn("CampaignId", typeof (Guid)),
        new DataColumn("ContactVisitIndex", typeof (int)),
        new DataColumn("DeviceId", typeof (Guid)),
        new DataColumn("LocationId", typeof (Guid)),
        new DataColumn("UserAgent", typeof (string)),
        new DataColumn("SiteName", typeof (string)),
        new DataColumn("Value", typeof (int)),
        new DataColumn("VisitPageCount", typeof (int)),
        new DataColumn("Ip", typeof (string)),
        new DataColumn("Keywords", typeof (string)),
        new DataColumn("ReferringSite", typeof (string)),
        new DataColumn("GeoData_BusinessName", typeof (string)),
        new DataColumn("GeoData_City", typeof (string)),
        new DataColumn("GeoData_Region", typeof (string)),
        new DataColumn("GeoData_Country", typeof (string))
      });
      return dataTable;
    }

    private void GetTableFromContactXconnect(DataTable rawTable, Guid contactID, Guid? interactionID = null)
    {
      string[] strArray = new string[3]
      {
        "IpInfo",
        "WebVisit",
        "UserAgentInfo"
      };
      ContactExpandOptions contactExpandOptions = new ContactExpandOptions(Array.Empty<string>())
      {
        Interactions = new RelatedInteractionsExpandOptions(strArray)
        {
          Limit = new int?(int.MaxValue),
          StartDateTime = new DateTime?(DateTime.MinValue)
        }
      };
      List<Interaction> list = Enumerable.ToList<Interaction>((IEnumerable<Interaction>) Enumerable.OrderByDescending<Interaction, DateTime>((IEnumerable<Interaction>) XdbContactServiceHelper.GetContactByOptions(contactID, (ExpandOptions) contactExpandOptions).Interactions, (Func<Interaction, DateTime>) (p => p.StartDateTime)));
      if (interactionID.HasValue)
      {
        Interaction curInteraction = Enumerable.FirstOrDefault<Interaction>((IEnumerable<Interaction>) list, (Func<Interaction, bool>) (p =>
        {
          Guid? id = p.Id;
          Guid guid = interactionID.Value;
          if (!id.HasValue)
            return false;
          if (!id.HasValue)
            return true;
          return id.GetValueOrDefault() == guid;
        }));
        int index = list.IndexOf(curInteraction) + 1;
        this.FillTableWithRow(rawTable, curInteraction, index);
      }
      else
      {
        foreach (Interaction curInteraction in list)
        {
          int index = list.IndexOf(curInteraction) + 1;
          this.FillTableWithRow(rawTable, curInteraction, index);
        }
      }
    }

    private void FillTableWithRow(DataTable rawTable, Interaction curInteraction, int index = 1)
    {
      WebVisit webVisit = CollectionModel.WebVisit(curInteraction);
      IpInfo ipInfo = CollectionModel.IpInfo(curInteraction);
      int count = Enumerable.ToList<PageViewEvent>(Enumerable.OfType<PageViewEvent>((IEnumerable) curInteraction.Events)).Count;
      DataRow row = rawTable.NewRow();
      row["ContactId"] = (object) curInteraction.Contact.Id;
      row["_id"] = (object) curInteraction.Id;
      row["ChannelId"] = (object) curInteraction.ChannelId;
      row["StartDateTime"] = (object) curInteraction.StartDateTime;
      row["EndDateTime"] = (object) curInteraction.EndDateTime;
      row["AspNetSessionId"] = (object) Guid.Empty;
      if (curInteraction.CampaignId.HasValue)
        row["CampaignId"] = (object) curInteraction.CampaignId;
      row["ContactVisitIndex"] = (object) index;
      row["DeviceId"] = (object) curInteraction.DeviceProfile.Id;
      row["LocationId"] = (object) Guid.Empty;
      row["UserAgent"] = (object) curInteraction.UserAgent;
      row["SiteName"] = (object) webVisit.SiteName;
      row["Value"] = (object) curInteraction.EngagementValue;
      row["VisitPageCount"] = (object) count;
      row["Ip"] = (object) ipInfo.IpAddress.ToString();
      row["Keywords"] = (object) webVisit.SearchKeywords;
      if (!string.IsNullOrEmpty(webVisit.Referrer))
      {
        Uri uri = new Uri(webVisit.Referrer);
        row["ReferringSite"] = (object) uri.Host;
      }
      row["GeoData_BusinessName"] = (object) ipInfo.BusinessName;
      row["GeoData_City"] = (object) ipInfo.City;
      row["GeoData_Region"] = (object) ipInfo.Region;
      row["GeoData_Country"] = (object) ipInfo.Country;
      rawTable.Rows.Add(row);
    }
  }
}
