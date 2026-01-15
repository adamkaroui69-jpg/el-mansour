using ElMansourSyndicManager.Core.Domain.Constants;
using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElMansourSyndicManager.ViewModels
{
    public class RolesViewModel : ViewModelBase
    {
        private readonly IPermissionService _permissionService;
        private bool _isEditing;
        private RoleDto? _selectedRole;
        private string _roleName = string.Empty;
        private string _roleDescription = string.Empty;
        private bool _isDialogOpen;

        public ObservableCollection<RoleDto> Roles { get; } = new ObservableCollection<RoleDto>();
        public ObservableCollection<PermissionGroupDto> PermissionGroups { get; } = new ObservableCollection<PermissionGroupDto>();

        public RoleDto? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    if (value != null)
                    {
                        LoadRoleForEdit(value);
                    }
                }
            }
        }

        public string RoleName
        {
            get => _roleName;
            set => SetProperty(ref _roleName, value);
        }

        public string RoleDescription
        {
            get => _roleDescription;
            set => SetProperty(ref _roleDescription, value);
        }

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => SetProperty(ref _isDialogOpen, value);
        }
        
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public ICommand LoadRolesCommand { get; }
        public ICommand AddRoleCommand { get; }
        public ICommand SaveRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly IDialogService _dialogService;

        public RolesViewModel(IPermissionService permissionService, IDialogService dialogService)
        {
            _permissionService = permissionService;
            _dialogService = dialogService;

            LoadRolesCommand = new RelayCommand(async () => await LoadRolesAsync());
            AddRoleCommand = new RelayCommand(PrepareAddRole);
            SaveRoleCommand = new RelayCommand(async () => await SaveRoleAsync(), CanSaveRole);
            DeleteRoleCommand = new RelayCommand(async () => await DeleteRoleAsync(), () => SelectedRole != null && !SelectedRole.IsSystem);
            CancelCommand = new RelayCommand(CloseDialog);

            // Charger les définitions de permissions
            InitializePermissionGroups();
        }

        private void InitializePermissionGroups()
        {
            var definitions = new Dictionary<string, string>
            {
                { AppPermissions.Payments.View, "Voir les paiements" },
                { AppPermissions.Payments.Create, "Encaisser un paiement" },
                { AppPermissions.Payments.Edit, "Modifier un paiement" },
                { AppPermissions.Payments.Delete, "Supprimer un paiement" },
                { AppPermissions.Payments.Validate, "Valider un paiement" },
                
                { AppPermissions.Expenses.View, "Voir les dépenses" },
                { AppPermissions.Expenses.Create, "Créer une dépense" },
                { AppPermissions.Expenses.Edit, "Modifier une dépense" },
                { AppPermissions.Expenses.Delete, "Supprimer une dépense" },

                { AppPermissions.Users.View, "Voir les résidents" },
                { AppPermissions.Users.Create, "Ajouter un résident" },
                { AppPermissions.Users.Edit, "Modifier un résident" },
                { AppPermissions.Users.Delete, "Supprimer un résident" },
                { AppPermissions.Users.ManageRoles, "Gérer les rôles" },
                
                { AppPermissions.Reports.View, "Consulter les rapports" },
                { AppPermissions.Reports.Export, "Exporter les rapports" },
                
                { AppPermissions.Documents.View, "Voir les documents" },
                { AppPermissions.Documents.Upload, "Ajouter des documents" },
                { AppPermissions.Documents.Delete, "Supprimer des documents" },
                
                { AppPermissions.System.ViewAuditLogs, "Voir le journal d'audit" },
                { AppPermissions.System.ManageSettings, "Modifier les paramètres" },
                { AppPermissions.System.ManageBackups, "Gérer les sauvegardes" }
            };

            var allPerms = AppPermissions.GetAll();
            var grouped = allPerms.GroupBy(p => p.Split('.')[0]);

            foreach (var group in grouped)
            {
                var groupDto = new PermissionGroupDto
                {
                    GroupName = group.Key,
                    Permissions = group.Select(p => new PermissionDto
                    {
                        Code = p,
                        DisplayName = definitions.TryGetValue(p, out var name) ? name : p,
                        IsAssigned = false
                    }).ToList()
                };
                PermissionGroups.Add(groupDto);
            }
        }

        public async Task LoadRolesAsync()
        {
            try
            {
                Roles.Clear();
                var roles = await _permissionService.GetAllRolesAsync();
                foreach (var role in roles)
                {
                    Roles.Add(new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description,
                        IsSystem = role.IsSystem,
                        UserCount = role.Users?.Count ?? 0,
                        Permissions = role.Permissions.Select(p => p.PermissionCode).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Erreur lors du chargement des rôles : {ex.Message}");
            }
        }

        private void PrepareAddRole()
        {
            SelectedRole = null;
            RoleName = "";
            RoleDescription = "";
            IsEditing = false;
            
            // Tout décocher
            foreach (var group in PermissionGroups)
                foreach (var perm in group.Permissions)
                    perm.IsAssigned = false;

            IsDialogOpen = true;
        }

        private void LoadRoleForEdit(RoleDto role)
        {
            if (role == null) return;
            
            RoleName = role.Name;
            RoleDescription = role.Description;
            IsEditing = true;

            // Cocher les permissions
            var rolePerms = new HashSet<string>(role.Permissions);
            foreach (var group in PermissionGroups)
            {
                foreach (var perm in group.Permissions)
                {
                    perm.IsAssigned = rolePerms.Contains(perm.Code);
                }
            }
            
            IsDialogOpen = true;
        }

        private bool CanSaveRole()
        {
            return !string.IsNullOrWhiteSpace(RoleName);
        }

        private async Task SaveRoleAsync()
        {
            if (string.IsNullOrWhiteSpace(RoleName)) return;

            try
            {
                var selectedPermissions = PermissionGroups
                    .SelectMany(g => g.Permissions)
                    .Where(p => p.IsAssigned)
                    .Select(p => p.Code)
                    .ToList();

                if (IsEditing && SelectedRole != null)
                {
                    // Update
                    var role = await _permissionService.GetRoleByIdAsync(SelectedRole.Id);
                    if (role != null)
                    {
                        role.Name = RoleName;
                        role.Description = RoleDescription;
                        await _permissionService.UpdateRoleAsync(role, selectedPermissions);
                        _dialogService.ShowMessage("Rôle mis à jour avec succès.");
                    }
                }
                else
                {
                    // Create
                    var newRole = new Role
                    {
                        Name = RoleName,
                        Description = RoleDescription,
                        IsSystem = false
                    };
                    await _permissionService.CreateRoleAsync(newRole, selectedPermissions);
                    _dialogService.ShowMessage("Nouveau rôle créé avec succès.");
                }

                IsDialogOpen = false;
                await LoadRolesAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Erreur lors de l'enregistrement : {ex.Message}");
            }
        }

        private async Task DeleteRoleAsync()
        {
            if (SelectedRole == null || SelectedRole.IsSystem) return;

            var confirmed = await _dialogService.ShowConfirmationAsync(
                $"Êtes-vous sûr de vouloir supprimer le rôle '{SelectedRole.Name}' ? Cette action est irréversible et affectera {SelectedRole.UserCount} utilisateur(s).",
                "Supprimer le rôle",
                "Supprimer"
            );

            if (!confirmed) return;

            try
            {
                await _permissionService.DeleteRoleAsync(SelectedRole.Id);
                _dialogService.ShowMessage("Rôle supprimé.", "Annuler"); // Undo potentiel futur
                await LoadRolesAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Erreur lors de la suppression : {ex.Message}");
            }
        }

        private void CloseDialog()
        {
            IsDialogOpen = false;
            SelectedRole = null;
        }
    }
}
