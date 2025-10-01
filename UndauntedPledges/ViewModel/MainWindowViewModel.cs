using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UndauntedPledges.Constants;
using UndauntedPledges.Enums;
using UndauntedPledges.Implementations;
using UndauntedPledges.Interfaces;
using UndauntedPledges.Models;

namespace UndauntedPledges.ViewModel;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IPledgeContext _pledgeContext;
    private readonly ApplicationSettings _settings;

    private DateTime? _selectedStartDate;
    private DateTime? _selectedEndDate;
    private bool _isLoading;
    private Role? _selectedRole;
    private string? _description;
    private string? _time;

    public ObservableCollection<DailyPledge> Pledges { get; } = [];

    public ObservableCollection<Role> Roles { get; } = [];

    public Role? SelectedRole
    {
        get => _selectedRole;
        set => SetField(ref _selectedRole, value);
    }

    public DelegateCommand SaveCommand { get; }

    public MainWindowViewModel(
        IPledgeContext pledgeContext,
        IOptions<ApplicationSettings> options)
    {
        _pledgeContext = pledgeContext;
        _settings = options.Value;

        SaveCommand = new DelegateCommand(SaveCommandHandler, CanExecuteSaveCommand);

        InitializeData();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public DateTime? SelectedStartDate
    {
        get => _selectedStartDate;
        set
        {
            SetField(ref _selectedStartDate, value);
            UpdatePledgesPreview();
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    public DateTime? SelectedEndDate
    {
        get => _selectedEndDate;
        set
        {
            SetField(ref _selectedEndDate, value);
            UpdatePledgesPreview();
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    public string? Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string? Time
    {
        get => _time;
        set
        {
            SetField(ref _time, value);
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private void InitializeData()
    {
        foreach (var role in _settings.Roles)
        {
            Roles.Add(role);
        }

        if (Roles.Count > 0)
        {
            SelectedRole = Roles[0];
        }

        SelectedStartDate = DateTime.Today;

        Description = """
                      Требования: 50+ лвл, можно без ДЛС 
                      **ПРИОРИТЕТ НОВЫМ ИГРОКАМ!!!**
                      -# Расскажу про механики, все объясню что делать
                      """;

        Time = "19:00";
    }

    private void UpdatePledgesPreview()
    {
        var startDate = SelectedStartDate;
        var endDate = SelectedEndDate;
        if (startDate is null || endDate is null)
        {
            return;
        }

        if (startDate > endDate)
        {
            return;
        }

        IsLoading = true;

        Task.Run(async () =>
        {
            var pledges = await _pledgeContext.GetDailyPledgeAsync(PledgeSource.EsoHub, startDate.Value, endDate.Value);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Pledges.Clear();

                foreach (var pledge in pledges.Where(x => x.DateTime.Date >= startDate && x.DateTime.Date <= endDate))
                {
                    Pledges.Add(pledge);
                }

                IsLoading = false;
            });
        });
    }

    private bool CanExecuteSaveCommand(object? parameter)
    {
        return SelectedStartDate is not null
               && SelectedEndDate is not null
               && SelectedStartDate <= SelectedEndDate
               && !string.IsNullOrEmpty(Time);
    }

    private void SaveCommandHandler(object? parameter)
    {
        if (Pledges.Count == 0)
        {
            return;
        }

        var openFileDialog = new SaveFileDialog
        {
            InitialDirectory = Directory.GetCurrentDirectory(),
            FileName = "output.txt",
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() is not true)
        {
            return;
        }

        var path = openFileDialog.FileName;
        var culture = new CultureInfo("ru-RU");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(fs);

        foreach (var pledge in Pledges)
        {
            var message = _settings.Template
                .Replace(ApplicationConstants.RolePlaceholder, SelectedRole?.Value ?? string.Empty)
                .Replace(ApplicationConstants.DatePlaceholder, pledge.DateTime.ToString("dd.MM.yyyy"))
                .Replace(ApplicationConstants.TimePlaceholder, Time ?? string.Empty)
                .Replace(ApplicationConstants.DayPlaceHolder, pledge.DateTime.ToString("ddd", culture))
                .Replace(ApplicationConstants.MajPledgePlaceHolder, pledge.MajPledge)
                .Replace(ApplicationConstants.GlirionPlaceHolder, pledge.GlirionPledge)
                .Replace(ApplicationConstants.UrgarlagPlaceHolder, pledge.UrgarlagPledge)
                .Replace(ApplicationConstants.DescriptionPlaceHolder, Description ?? string.Empty);

            writer.WriteLine(message);
            writer.Write(Environment.NewLine + Environment.NewLine);
        }

        MessageBox.Show("Готово");
    }
}