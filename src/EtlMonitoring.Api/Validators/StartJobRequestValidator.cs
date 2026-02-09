using FluentValidation;
using EtlMonitoring.Core.DTOs;

namespace EtlMonitoring.Api.Validators
{
    public class StartJobRequestValidator : AbstractValidator<StartJobRequest>
    {
        public StartJobRequestValidator()
        {
            RuleFor(x => x.JobName)
                .NotEmpty().WithMessage("Nome do job é obrigatório")
                .MaximumLength(200).WithMessage("Nome do job deve ter no máximo 200 caracteres")
                .Matches(@"^[a-zA-Z0-9_\-\.]+$").WithMessage("Nome do job contém caracteres inválidos");
        }
    }

    public class FinishJobRequestValidator : AbstractValidator<FinishJobRequest>
    {
        public FinishJobRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status é obrigatório")
                .Must(status => new[] { "Sucesso", "Falha", "Parcial", "Cancelado" }.Contains(status))
                .WithMessage("Status deve ser: Sucesso, Falha, Parcial ou Cancelado");

            RuleFor(x => x.ErrorMessage)
                .MaximumLength(4000).WithMessage("Mensagem de erro muito longa (máx 4000 caracteres)")
                .When(x => !string.IsNullOrEmpty(x.ErrorMessage));
        }
    }

    public class JobExecutionFiltrosDtoValidator : AbstractValidator<JobExecutionFiltrosDto>
    {
        public JobExecutionFiltrosDtoValidator()
        {
            RuleFor(x => x.Limite)
                .GreaterThan(0).WithMessage("Limite deve ser maior que 0")
                .LessThanOrEqualTo(1000).WithMessage("Limite máximo é 1000");

            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || new[] { "Sucesso", "Falha", "Parcial", "EmExecucao", "Cancelado" }.Contains(status))
                .WithMessage("Status inválido")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("Data final deve ser maior que data inicial")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }
    }
}
