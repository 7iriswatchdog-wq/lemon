using AML.DTO.DTO.SectoralTMS;
using AML.ViewModel.ViewModels.SectoralTMS;
using AutoMapper;

namespace AML.Web.Mappings
{
    public class StmMappingProfile : Profile
    {
        public StmMappingProfile()
        {
            CreateMap<StmSectorDTO, StmSectorModel>().ReverseMap();

            CreateMap<StmRuleDTO, StmRuleModel>().ReverseMap();
            CreateMap<StmRuleConditionDTO, StmRuleConditionModel>().ReverseMap();

            CreateMap<StmTransactionDTO, StmTransactionModel>().ReverseMap();
            CreateMap<StmTransactionPartyDTO, StmTransactionPartyModel>().ReverseMap();

            CreateMap<StmCaseDTO, StmCaseModel>().ReverseMap();
            CreateMap<StmCaseCommentDTO, StmCaseCommentModel>().ReverseMap();
        }
    }
}
