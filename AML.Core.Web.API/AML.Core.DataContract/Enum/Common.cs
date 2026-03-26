using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.Core.DataContract.Enum
{
    public enum ItemType
    {
        department = 1,
        clientcase = 2,
        freeSource = 3,
        caseDocument = 4,
        InternalWatchList = 5,
        Logo = 6,
        IndividualUpload = 7
    }
    public enum EnvironmentEnum
    {
        Development = 1,
        Production = 2
    }

    public enum ExceptionTypeMesage
    {
        Warning = 1,
        Info = 2,
        Error = 3
    }

    public enum OperationType
    {
        Copy = 1,
        CSV = 2,
        Excel = 3,
        PDF = 4,
        Print = 5
    }

    public enum MatchingType
    {
        All = 1,
        Matched = 2,
        NotMatched = 3,
    }
    public enum CaseStatus
    {
        All = 10,
        Pending = 0,
        PendingCaseCreatedFromDailyScheduler = 6,
        Approved = 2,
        Rejected = 3,
        Auto = 5,
    }
    public enum ReportsCaseStatus
    {
        All = 10,
        Pending = 0,
        PendingCaseCreatedFromDailyScheduler = 6,
        PendingWithSeniorManagement=4,
        Approved = 2,
        Rejected = 3,
        Auto = 5,
    }
    public enum CompletedCaseStatus
    {

        All = 10,
        Approved = 2,
        Rejected = 3,
    }

    public enum SourceType
    {
        //OFAC = 0,
        //UN = 1,
        //CBList = 2,
        InternalWatchlist = 3
    }
    public enum LogModulle
    {
        Customer = 1,
        InternalWatchlist = 2
    }
    public enum Profession
    {
        SalariedinPrivateSector = 2,
        SelfEmployed = 2,
        SalariedinPublicSector = 1,
        Freelancer = 3

    }

    public enum ResidenceStatus
    {
        Resident = 1,
        NonResident = 2
    }

    public enum YesNo
    {
        Yes = 1,
        No = 0
    }

    public enum ProductorService
    {
        FC = 2,
        Remittance = 3,
        FCandRemittanceBoth = 3
    }

    public enum DeliveryChannel
    {
        FacetoFace = 1,
        NonFacetoFace = 3
    }

    public enum Customersbehavior
    {
        Normal = 1,
        RecommendedforMonitoredCustomer = 3
    }
    //public static class FileUpload
    //{
    //    public static string BasePathUploadFolder = "upload";
    //    public static string FileUploadPath = "c:/amlstorage/";
    //}
    public enum TranCaseStatus
    {
        Pending = 0,
        //Assigned = 1,
        Approved = 2,
        Rejected = 3,
        Closed = 4,
        AutoApproved = 5,
        Invalid = 10,

    }
}
