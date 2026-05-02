namespace ChargePoint.CarManagement.Domain.ViewModels
{
    public class SystemSettingItemVM
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = "boolean";

        // Custom UI states logic pulled from the view
        public bool IsDisabled { get; set; }
        public string DisabledReason { get; set; } = string.Empty;
    }

    public class SystemSettingIndexVM
    {
        public List<SystemSettingItemVM> Settings { get; set; } = new List<SystemSettingItemVM>();
        public bool IsRootUser { get; set; }
    }
}
