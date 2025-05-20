$(document).ready(function () {
    const usersTable = $('#tableUser').DataTable({
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/pl.json',
        },
        autoWidth: false,
        scrollCollapse: true,
        scrollX: true,
        ajax: {
            url: '/AdminPanel/Users/GetAllUsers',
            type: "POST",
            dataType: "json"
        },
        columns: [
            {
                data: "userId",
                name: "ID",
                render: function (data) {
                    if (data.length <= 6) return data;
                    return data.slice(0, 3) + '...' + data.slice(-3);
                }
            },
            { data: "email", name: "Email", },
            { data: "userName", name: "Nazwa użytkownika", },
            {
                data: "roleName",
                name: "Rola",
                render: function (data) {
                    if (data === "Admin") {
                        return '<span class="badge badge-admin">Administrator</span>';
                    } else {
                        return '<span class="badge badge-user">Użytkownik</span>';
                    }
                }
            },
            {
                data: "isLocked",
                name: "Status",
                render: function (data) {
                    if (data === true) {
                        return '<span class="badge badge-locked">Zablokowany</span>';
                    } else {
                        return '<span class="badge badge-active">Aktywny</span>';
                    }
                }
            },
            {
                data: "userId",
                name: "Akcje",
                orderable: false,
                className: "actions",
                render: function (data, type, row) {
                    return `
                    	<div class="dropdown text-center">
                        	<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownMenuButton-${data}" data-bs-toggle="dropdown" aria-expanded="false">
                            	<i class="bi bi-list  fs-4"></i>
                        	</button>
                        	<ul class="dropdown-menu" aria-labelledby="dropdownMenuButton-${data}">
                            	<li>
                                	<button class="dropdown-item edit-user" data-id="${data}" type="button">
                                    	<i class="fas fa-edit"></i> Edytuj użytkownika
                                	</button>
                            	</li>
                            	<li>
                                	${row.isLocked ?
                            `<form method="post" action="/AdminPanel/Users/UnlockUser/${data}" style="margin:0">
                                        	<button type="submit" class="dropdown-item unlock-user">
                                            	<i class="fas fa-unlock"></i> Odblokuj użytkownika
                                        	</button>
                                    	</form>` :
                            `<form method="post" action="/AdminPanel/Users/LockUser/${data}" style="margin:0">
                                        	<button type="submit" class="dropdown-item lock-user">
                                            	<i class="fas fa-lock"></i> Zablokuj użytkownika
                                        	</button>
                                     	</form>`
                        }
                            	</li>
                        	</ul>
                    	</div>`;
                }
            }
        ],
        columnDefs: [
            { "targets": 0, "width": "10%" },
            { "targets": 1, "width": "20%" },
            { "targets": 2, "width": "20%" },
            { "targets": 3, "width": "15%" },
            { "targets": 4, "width": "15%" },
            { "targets": 5, "width": "20%", "className": "text-center" }
        ],
        order: [[0, "asc"]]
    });


    $(document).on('click', '.dropdown-item.edit-user', function (event) {
        const userId = $(this).data('id');
        const url = '/AdminPanel/Users/GetUserById?userId=' + userId;


        $.ajax({
            url: url,
            type: 'GET',
            success: function (response) {
                const modal = $('#editUserModal');

                modal.find('#EditId').val(response.userId);
                modal.find('#EditEmail').val(response.email);
                modal.find('#EditUserName').val(response.userName);
                modal.find('#EditRoleId').val(response.roleId);

                const modalInstance = new bootstrap.Modal(document.getElementById('editUserModal'));
                modalInstance.show();
            },
            error: function (error) {
                console.error('Error fetching user data:', error);
                alert('Failed to fetch user data. Please try again.');
            }
        });
    });
});



