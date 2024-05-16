namespace performance_monitor_winui3.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
