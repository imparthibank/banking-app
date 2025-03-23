using AutoMapper;
using BankingApp.Core.Entities;
using BankingApp.Application.DTOs;

namespace BankingApp.WebApi.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreateBankAccountRequest, BankAccount>().ReverseMap();
        }
    }
}
