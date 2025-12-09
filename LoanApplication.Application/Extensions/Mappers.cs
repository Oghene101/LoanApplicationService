using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Filters;
using LoanApplication.Application.Features.Auth.Commands;
using LoanApplication.Application.Features.Kyc.Commands;
using LoanApplication.Application.Features.LoanApplication.Commands;
using LoanApplication.Application.Features.LoanApplicationAdmin.Commands;
using LoanApplication.Application.Features.LoanApplicationAdmin.Queries;
using LoanApplication.Domain.Entities;
using RefreshToken = LoanApplication.Application.Features.Auth.Commands.RefreshToken;
using LoanApp = LoanApplication.Application.Common.Contracts.LoanApplication;


namespace LoanApplication.Application.Extensions;

public static class Mappers
{
    #region To Entity

    public static User ToEntity(this SignUp.Command dto)
        => new()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            CreatedBy = $"{dto.FirstName} {dto.LastName}",
            UpdatedBy = $"{dto.FirstName} {dto.LastName}"
        };

    public static Domain.Entities.LoanApplication ToEntity(this ApplyForLoan.Command dto, User user)
        => new()
        {
            Amount = dto.Amount,
            Tenure = dto.Tenure,
            Purpose = dto.Purpose,
            UserId = user.Id,
            CreatedBy = $"{user.FirstName} {user.LastName}",
            UpdatedBy = $"{user.FirstName} {user.LastName}"
        };

    public static Address ToEntity(this AddAddress.Command dto)
        => new()
        {
            HouseNumber = dto.HouseNumber,
            Landmark = dto.Landmark,
            Street = dto.Street,
            Lga = dto.Lga,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
        };

    #endregion

    #region To Vm

    #region Loan Application Admin

    public static LoanApplicationAdmin.GetPendingLoanApplicationsResponse ToVm(this Domain.Entities.LoanApplication dto)
        => new(
            dto.Id,
            dto.User.FirstName,
            dto.User.LastName,
            dto.User.Email!,
            dto.Amount,
            dto.Tenure,
            dto.Purpose,
            dto.CreatedAt);

    #endregion

    #endregion

    #region To Command

    #region Auth

    public static SignUp.Command ToCommand(this Auth.SignUpRequest dto)
        => new(dto.FirstName, dto.LastName, dto.Email, dto.Password);

    public static ConfirmEmail.Command ToCommand(this Auth.ConfirmEmailRequest dto)
        => new(dto.Email, dto.Token);

    public static SignIn.Command ToCommand(this Auth.SignInRequest dto)
        => new(dto.Email, dto.Password);

    public static RefreshToken.Command ToCommand(this Auth.RefreshTokenRequest dto)
        => new(dto.AccessToken, dto.RefreshToken);

    public static ChangePassword.Command ToCommand(this Auth.ChangePasswordRequest dto)
        => new(dto.OldPassword, dto.NewPassword);

    public static ForgotPassword.Command ToCommand(this Auth.ForgotPasswordRequest dto)
        => new(dto.Email);

    public static ResetPassword.Command ToCommand(this Auth.ResetPasswordRequest dto)
        => new(dto.Email, dto.Token, dto.NewPassword);

    #endregion

    #region User

    public static AddBvn.Command ToCommand(this Kyc.AddBvnRequest dto)
        => new(dto.Bvn);

    public static AddNin.Command ToCommand(this Kyc.AddNinRequest dto)
        => new(dto.Nin);

    public static AddAddress.Command ToCommand(this Kyc.AddAddressRequest dto)
        => new(dto.HouseNumber, dto.Landmark, dto.Street, dto.Lga, dto.City, dto.State, dto.Country);

    #endregion

    #region Loan Application

    public static ApplyForLoan.Command ToCommand(this LoanApp.ApplyForLoanRequest dto)
        => new(dto.Amount, dto.Tenure, dto.Purpose);

    #endregion

    #region Loan Application Admin

    public static ReviewLoanApplication.Command ToCommand(
        this LoanApplicationAdmin.ReviewLoanApplicationRequest dto)
        => new(
            dto.LoanApplicationId,
            dto.Email,
            dto.FirstName,
            dto.LastName,
            dto.LoanApplicationStatus,
            dto.Comment);

    #endregion

    // #region Admin
    //
    // public static SendEmailConfirmation.Command ToCommand(this Admin.SendEmailConfirmationRequest dto)
    //     => new(dto.Email);
    //
    // public static FastForwardLockoutEnd.Command ToCommand(this Admin.FastForwardLockOutEndRequest dto)
    //     => new(dto.Email);
    //
    // #endregion

    #endregion

    #region To Query

    #region Loan Application

    public static GetPendingLoanApplications.Query ToQuery(
        this LoanApplicationAdmin.GetPendingLoanApplicationsRequest dto)
        => new(
            new PaginationFilter(dto.PageNumber, dto.PageSize),
            new DateRangeFilter(dto.StartDate, dto.EndDate));

    #endregion

    #endregion
}