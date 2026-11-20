
document.addEventListener('DOMContentLoaded', () => {
    async function refreshSessionIfNeeded() {
        if (typeof refreshSession === 'function') {
            await refreshSession(false);
        }
    }

    async function toggleRolPermission(roleId, permissionId, action) {
        const url = action === 'assign'
            ? `/Admin/AssignPermissionToRole?roleId=${roleId}&permissionId=${permissionId}`
            : `/Admin/RemovePermissionFromRole?roleId=${roleId}&permissionId=${permissionId}`;

        const response = await axios.post(url);
        return response.data;  
    }

    document.querySelectorAll('.btn-toggle-permission').forEach(button => {
        button.addEventListener('click', async () => {
            try {
                const result = await toggleRolPermission(
                    button.dataset.roleId,
                    button.dataset.permissionId,
                    button.dataset.action);

                if (result.success) {
                    await refreshSessionIfNeeded();
                    await Swal.fire({ icon: "success", title: 'Listo', text: result.message, timer: 1500, showConfirmButton: false });
                    location.reload();
                }
            } catch {
                Swal.fire({icon: "error", title: 'Error', text: 'No se pudo completar la operación'});
            }
        })
    });

    const assignBtn = document.getElementById('btnAssignPermission');
    const permissionSelect = document.getElementById('permissionToAssign');
    if (assignBtn && permissionSelect) {
        assignBtn.addEventListener('click', async () => {
            try {
                const result = await toggleRolPermission(
                    assignBtn.dataset.roleId,
                    permissionSelect.value,
                    'assign');

                if (result.success) {
                    await refreshSessionIfNeeded();
                    await Swal.fire({ icon: "success", title: 'Listo', text: result.message, timer: 1500, showConfirmButton: false });
                    location.reload();
                } else {
                    Swal.fire({ icon: "error", title: 'Error', text: result.message });
                }
            } catch {
                Swal.fire({ icon: "error", title: 'Error', text: 'No se pudo asignar el permiso' });
            }
        });
    }

    const updateRoleBtn = document.getElementById('btnUpdateRole');
    if (updateRoleBtn) {
        updateRoleBtn.addEventListener('click', async () => {
            const userId = updateRoleBtn.dataset.userId;
            const roleId = document.getElementById('userRoleSelect').value;

            try {
                const response = await axios.post(`/Admin/UpdateUserRole?userId=${userId}`, { roleId: parseInt(roleId) })
                if (response.data.success) {
                    await refreshSessionIfNeeded();
                    await Swal.fire({ icon: "success", title: 'Rol Actualizado', text: response.data.message, timer: 1500, showConfirmButton: false });
                } else {
                    Swal.fire({ icon: "error", title: 'Error', text: response.data.message });
                }
            } catch {
                Swal.fire({ icon: "error", title: 'Error', text: 'No se pudo actualizar el rol' });
            }
        });
    }

    async function assigUserPermission(userId, permissionId, isGranted) {
        const response = await axios.post(`/Admin/AssignPermissionToUser?userId=${userId}`, {
            permissionId: parseInt(permissionId),
            isGranted
        });
        return response.data;
    }

    const userPermissionsTable = document.getElementById('userPermissionsTable');
    if (userPermissionsTable) {
        userPermissionsTable.addEventListener('click', async (event) => {
            const grantBtn = event.target.closest('.btn-user-grant');
            const denyBtn = event.target.closest('.btn-user-deny');
            const removeBtn = event.target.closest('.btn-user-remove');
            const button = grantBtn || denyBtn || removeBtn;
            if (!button) return;

            try {
                if (grantBtn) {
                    const result = await assigUserPermission(button.dataset.userId, button.dataset.permissionId, true);
                    if (result.success) {
                        await refreshSessionIfNeeded();
                        await Swal.fire({ icon: "success", title: 'Listo', text: result.message, timer: 1500, showConfirmButton: false });
                        location.reload();
                    } else {
                        Swal.fire({ icon: "error", title: 'Error', text: result.message });
                    }
                }

                if (denyBtn) {
                        const result = await assigUserPermission(button.dataset.userId, button.dataset.permissionId, false);
                        if (result.success) {
                            await refreshSessionIfNeeded();
                            await Swal.fire({ icon: "success", title: 'Listo', text: result.message, timer: 1500, showConfirmButton: false });
                            location.reload();
                        } else {
                            Swal.fire({ icon: "error", title: 'Error', text: result.message });
                        }
                    }

                if (removeBtn) {
                    const response = await axios.post(`/Admin/RemoveUserPermission?userId=${button.dataset.userId}&permissionId=${button.dataset.permissionId}`);
                    if (response.data.success) {
                        await refreshSessionIfNeeded();
                        await Swal.fire({ icon: "success", title: 'Listo', text: response.data.message, timer: 1500, showConfirmButton: false });
                        location.reload();
                    } else {
                        Swal.fire({ icon: "error", title: 'Error', text: response.data.message });
                    }
                }
            } catch {
                Swal.fire({ icon: "error", title: 'Error', text: 'No se pudo completar la operación' });
            }
        })
    }
});
