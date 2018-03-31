var timetable = Vue.component('timetable',
    {
        template: '#timetable-template',
        data: function () {
            return {
                messageText: null,
                messageClass: '',
                currentDay: null,
                selectedTimetable: null,
                createTimetable: null,
                groups: [],
                disciplines: [],
                teachers: [],
                allTimetables: [],
                
            };
        },
        methods: {
            setCurrentDay: function (date) {
                this.currentDay = moment(date).format("YYYY-MM-DD");
                if (this.createTimetable != null) {
                    this.createTimetable.date = this.currentDay;
                } else if (this.selectedTimetable != null) {
                    this.selectedTimetable.Date = this.currentDay;
                }
                else {
                    this.getTimeTables();
                }
            },
            selectTimetable: function (timetable) {
                if (!this.$parent.user.IsInRoleAdmin) {
                    return;
                }

                this.selectedTimetable = {
                    Id: timetable.Id,
                    Date: moment(timetable.Date).format("YYYY-MM-DD"),
                    StartTime: moment(timetable.StartTime).format("HH:mm"),
                    ExpirationTime: moment(timetable.ExpirationTime).format("HH:mm"),
                    DisciplineId: timetable.DisciplineId,
                    TeacherId: timetable.TeacherId,
                    StudentGroupId: timetable.StudentGroupId
                };
            },
            createNewTimetable: function () {
                this.createTimetable = {
                    date: this.currentDay,
                    startTime: "08:30",
                    expirationTime: "09:50",
                    disciplineId: "",
                    teacherId: "",
                    studentGroupId: ""
                }
            },
            postTimetable: function () {
                this.$http.post('/api/Timetables/Post', this.createTimetable,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.createNewTimetable();
                        this.showInfo('alert-success', 'Расписание создано');
                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        console.log(error);
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    }
                    );
            },
            saveTimetable: function () {
                this.$http.put('/api/Timetables/Put/' + this.selectedTimetable.Id, this.selectedTimetable, 
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.showInfo('alert-success', 'Расписание отредактировано');
                        this.showTimetables();
                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    });
            },
            removeTimetable: function (timetable) {
                if (!confirm('Вы действительно хотите удалить это расписание?')) {
                    return;
                }
                this.$http.delete('/api/Timetables/Delete/' + timetable.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showTimetables();
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', error.data.Message);
                }
                );
            },
            showTimetables: function () {
                this.createTimetable = null;
                this.selectedTimetable = null;
                this.messageText = null;
                this.getTimeTables();
            },
            loadGroups: function () {
                this.$http.get('/api/StudentGroups', { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        if (response.data[i].Id !== '00000000-0000-0000-0000-000000000000') {
                            this.groups.push(response.data[i]);
                        }
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadDisciplines: function () {
                this.$http.get('/api/Disciplines',
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        this.disciplines.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadTeachers: function () {
                this.$http.get('/api/Teachers',
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        this.teachers.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            getTimeTables: function () {
                var params = { date: this.currentDay };
                this.$http.get('/api/Timetables/GetTimetables',
                    { params: params, headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.allTimetables.splice(0, this.allTimetables.length);
                    for (var i = 0; i < response.data.length; i++) {
                        this.allTimetables.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            showInfo: function (messageClass, messageText) {
                this.messageText = messageText;
                this.messageClass = messageClass;
            },
            modelStateToString: function (modelState) {
                var text = '';

                for (var key in modelState) {
                    text += modelState[key].join(' ');
                }
                return text;
            },
            getGroupName: function (timetables) {
                var groupId = timetables[0].StudentGroupId;
                var group = findObjectByKey(this.groups, 'Id', groupId);
                if (group === null) {
                    return 'Ошибка загрузки данных.';
                }
                return group.Name;
            },
            getDisciplineName: function (disciplineId) {
                var discipline = findObjectByKey(this.disciplines, 'Id', disciplineId);
                if (discipline === null) {
                    return 'Ошибка загрузки данных.';
                }
                return discipline.Name;
            },
            getTeacherName: function (teacherId) {
                var teacher = findObjectByKey(this.teachers, 'Id', teacherId);
                if (teacher === null) {
                    return 'Ошибка загрузки данных.';
                }
                return teacher.FullName;
            }
        },
        mounted: function () {
            this.$nextTick(function () {
                var context = this;
                $('#date-picker').datepicker({
                    weekStart: 1,
                    maxViewMode: 2,
                    todayBtn: "linked",
                    language: "ru",
                    todayHighlight: true
                });
                $('#date-picker').datepicker()
                    .on("changeDate", function (event) {
                        context.setCurrentDay(event.date);
                    });
            });
        },
        created: function () {
            
            this.loadGroups();
            this.loadDisciplines();
            this.loadTeachers();
            this.setCurrentDay(moment());
        },
        filters: {
            formatedDate: function (date) {
                return moment(date).format("DD.MM.YYYY");
            },
            formatedTime: function (date) {
                return moment(date).format("HH:mm");
            }
        },
        computed: {
            userIsAdmin: function () {
                return this.$parent.user.IsInRoleAdmin;                
            }
        }
    });

var users = Vue.component('users',
    {
        template: '#users-template',
        data: function () {
            return {
                searchText: '',
                users: [],
                groups: [],
                currentUser: null,
                createUser: null,
                messageText: null,
                messageClass: '',
                newPassword: '',
                newPasswordConfirm: '',
                passwordInputType: 'password'
            };
        },
        methods: {
            selectUser: function (user) {
                this.currentUser = user;
                this.createUser = null;
                this.messageText = null;
                this.passwordInputType = 'password';
            },
            createNewUser: function (byClickButton) {
                if (byClickButton) {
                    this.messageText = null;
                }
                this.currentUser = null;
                this.createUser = {
                    userName: '',
                    password: '',
                    confirmPassword: '',
                    fullName: '',
                    email: '',
                    studentGroupId: '00000000-0000-0000-0000-000000000000',
                    role: 'Student'
                };
            },
            postNewUser: function () {
                this.$http.post('/api/Account/Register', this.createUser,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.users.push(response.data);
                    this.showInfo('alert-success', 'Пользователь ' + response.data.UserName + ' создан.');
                    this.createNewUser(false);
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                }
                );
            },
            saveUser: function (user) {
                this.$http.put('/api/Users/' + user.Id, user,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showInfo('alert-success', 'Пользователь ' + user.UserName + ' изменен.');
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                });
            },
            removeUser: function (user) {
                if (!confirm('Вы действительно хотите удалить пользователя ' + user.UserName)) {
                    return;
                }
                this.$http.delete('/api/Users/' + user.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showInfo('alert-warning', 'Пользователь ' + response.data.UserName + ' удален.');
                    var removeIndex = findObjectIndexByKey(this.users, 'Id', this.currentUser.Id);
                    if (removeIndex > -1) {
                        this.users.splice(removeIndex, 1);
                    }
                    this.currentUser = null;
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', error.data.Message);
                }
                );
            },
            showInfo: function (messageClass, messageText) {
                this.messageText = messageText;
                this.messageClass = messageClass;
            },
            modelStateToString: function (modelState) {
                var text = '';

                for (var key in modelState) {
                    text += modelState[key].join(' ');
                }
                return text;
            },
            generatePassword: function () {
                var chars = [
        "ABCDEFGHJKLMNPQRSTUVWXYZ",
        "abcdefghijkmnopqrstuvwxyz",
        "0123456789",
        "@#&-[{}]?/*_=+"
                ];
                var lengthPass = 8;
                var password = "";
                var randomIndex;
                var randomChar;
                for (var i = 0; i < lengthPass; i++) {
                    if (i < 4) {
                        randomIndex = i;
                    } else {
                        randomIndex = Math.floor(Math.random() * (chars.length - 1));
                    }
                    randomChar = Math.floor(Math.random() * (chars[randomIndex].length - 0));
                    password += chars[randomIndex][randomChar];
                }
                this.newPassword = password,
                this.newPasswordConfirm = password,
                this.passwordInputType = 'text';
            },
            setNewPassword: function (userId) {
                var setPassword = {
                    UserId: userId,
                    NewPassword: this.newPassword,
                    ConfirmPassword: this.newPasswordConfirm
                };
                this.$http.post('api/Account/SetPassword', setPassword,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showInfo('alert-success', 'Пароль изменен.');
                    this.newPassword = '';
                    this.newPasswordConfirm = '';
                    this.passwordInputType = 'password';
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                });
            },
            loadUsers: function () {
                this.$http.get('/api/Users', { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        this.users.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadGroups: function () {
                this.$http.get('/api/StudentGroups', { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        this.groups.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            }

        },
        computed: {
            filteredUsers: function () {
                function filter(obj) {
                    var isFinded = true;
                    var searchText = this.searchText.toLowerCase();
                    if (obj.FullName !== null) {
                        isFinded = obj.FullName.toLowerCase().indexOf(searchText) >= 0;
                        if (isFinded) {
                            return true;
                        }
                    }
                    if (obj.UserName !== null) {
                        isFinded = obj.UserName.toLowerCase().indexOf(searchText) >= 0;
                        if (isFinded) {
                            return true;
                        }
                    }
                    if (obj.Email !== null) {
                        isFinded = obj.Email.toLowerCase().indexOf(searchText) >= 0;
                        if (isFinded) {
                            return true;
                        }
                    }
                    return isFinded;
                }
                return this.users.filter(filter, this);
            }
        },
        created: function () {
            this.loadUsers();
            this.loadGroups();
        }
    });

var groups = Vue.component('groups',
    {
        template: '#groups-template',
        data: function () {
            return {
                searchText: '',
                groups: [],
                candidates: [],
                newParticipants: [],
                currentGroup: null,
                createGroup: null,
                messageText: null,
                messageClass: '',
                startDate: '',
                expirationDate: ''
            };
        },
        methods: {
            selectGroup: function (group) {
                this.currentGroup = group;
                this.createGroup = null;
                this.messageText = null;
                this.updateGroup(group);
                this.startDate = moment(this.currentGroup.StartDate).format('DD.MM.YYYY');
                this.expirationDate = moment(this.currentGroup.ExpirationDate).format('DD.MM.YYYY');
                this.loadCandidates();
            },
            createNewGroup: function () {
                this.currentGroup = null;
                var currentDate = moment();
                var futureDate = moment(currentDate).add(9, 'M');
                this.startDate = currentDate.format('DD.MM.YYYY');
                this.expirationDate = futureDate.format('DD.MM.YYYY');
                this.createGroup = {
                    name: ''
                };
            },
            postNewGroup: function () {

                var newGroup = {
                    name: this.createGroup.name,
                    startDate: moment(this.startDate, "DD-MM-YYYY").format("YYYY-MM-DD"),
                    expirationDate: moment(this.expirationDate, "DD-MM-YYYY").format("YYYY-MM-DD")
                };

                this.$http.post('/api/StudentGroups', newGroup,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.groups.push(response.data);
                        this.selectGroup(response.data);
                        this.showInfo('alert-success', 'Группа ' + response.data.Name + ' создана.');

                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    }
                    );
            },
            saveGroup: function () {
                var group = {
                    Id: this.currentGroup.Id,
                    Name: this.currentGroup.Name,
                    StartDate: moment(this.startDate, "DD-MM-YYYY").format("YYYY-MM-DD"),
                    ExpirationDate: moment(this.expirationDate, "DD-MM-YYYY").format("YYYY-MM-DD"),
                    ParticipantIds: this.newParticipants
                };

                this.$http.put('/api/StudentGroups/' + group.Id, group,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.showInfo('alert-success', 'Группа ' + group.Name + ' отредактирована.');
                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    }
                    );
            },
            removeGroup: function (group) {
                if (!confirm('Вы действительно хотите удалить группу ' + group.Name)) {
                    return;
                }
                this.$http.delete('/api/StudentGroups/' + group.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showInfo('alert-warning', 'Группа ' + group.Name + ' удалена.');
                    var removeIndex = findObjectIndexByKey(this.groups, 'Id', group.Id);
                    if (removeIndex > -1) {
                        this.groups.splice(removeIndex, 1);
                    }
                    this.currentGroup = null;
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', error.data.Message);
                }
                );
            },
            showInfo: function (messageClass, messageText) {
                this.messageText = messageText;
                this.messageClass = messageClass;
            },
            modelStateToString: function (modelState) {
                var text = '';

                for (var key in modelState) {
                    text += modelState[key].join(' ');
                }
                return text;
            },
            updateGroup: function (group) {
                this.$http.get('/api/StudentGroups/' + group.Id, { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.currentGroup.Students = response.data.Students;
                    this.newParticipants.splice(0, this.newParticipants.length);
                    for (var i = 0; i < this.currentGroup.Students.length; i++) {
                        this.newParticipants.push(this.currentGroup.Students[i].Id);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadCandidates: function () {
                this.$http.get('/api/GroupCandidates', { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.candidates.splice(0, this.candidates.length);
                    for (var i = 0; i < response.data.length; i++) {
                        this.candidates.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadGroups: function () {
                this.$http.get('/api/StudentGroups', { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        if (response.data[i].Id !== '00000000-0000-0000-0000-000000000000') {
                            this.groups.push(response.data[i]);
                        }
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            }

        },
        computed: {
            filteredGroups: function () {
                function filter(obj) {
                    var searchText = this.searchText.toLowerCase();
                    if (obj.Name !== null) {
                        return obj.Name.toLowerCase().indexOf(searchText) >= 0;
                    }
                    return true;
                }
                return this.groups.filter(filter, this);
            }
        },
        created: function () {
            this.loadGroups();
        },
        updated: function () {
            this.$nextTick(function () {
                var context = this;
                var pickerOptions = {
                    format: "dd.mm.yyyy",
                    weekStart: 0,
                    maxViewMode: 2,
                    todayBtn: "linked",
                    language: "ru",
                    orientation: "bottom auto",
                    autoclose: true
                };
                $('#start-date').datepicker(pickerOptions);
                $('#end-date').datepicker(pickerOptions);
                $("#start-date").datepicker()
                    .on("changeDate", function () { context.startDate = $('#start-date').val(); });
                $("#end-date").datepicker()
                    .on("changeDate", function () { context.expirationDate = $('#end-date').val(); });

            });
        }
    });

var teachers = Vue.component('teachers',
    {
        template: '#teachers-template',
        data: function () {
            return {
                searchText: '',
                disciplines: [],
                teachers: [],
                newDisciplines: [],
                currentTeacher: null,
                createTeacher: null,
                messageText: null,
                messageClass: ''
            };
        },
        methods: {
            selectTeacher: function (teacher) {
                this.currentTeacher = teacher;
                this.createTeacher = null;
                this.messageText = null;
                this.updateTeacher(teacher);
            },
            createNewTeacher: function () {
                this.currentTeacher = null;
                this.createTeacher = {
                    name: '',
                    phone: ''
                };
            },
            postNewTeacher: function () {
                this.$http.post('/api/Teachers', this.createTeacher,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.teachers.push(response.data);
                        this.selectTeacher(response.data);
                        this.showInfo('alert-success', 'Преподаватель ' + response.data.FullName + ' создан.');

                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    });
            },
            saveTeacher: function () {
                var updatedTeacher = {
                    Id: this.currentTeacher.Id,
                    FullName: this.currentTeacher.FullName,
                    Phone: this.currentTeacher.Phone,
                    DisciplineIds: this.newDisciplines
                }
                this.$http.put('/api/Teachers/' + updatedTeacher.Id, updatedTeacher,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.showInfo('alert-success', 'Преподаватель ' + updatedTeacher.FullName + ' отредактирован.');
                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    });
            },
            removeTeacher: function (teacher) {
                if (!confirm('Вы действительно хотите удалить преподавателя ' + teacher.FullName)) {
                    return;
                }
                this.$http.delete('/api/Teachers/' + teacher.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showInfo('alert-warning', 'Преподаватель ' + teacher.Name + ' удален.');
                    var removeIndex = findObjectIndexByKey(this.teachers, 'Id', teacher.Id);
                    if (removeIndex > -1) {
                        this.teachers.splice(removeIndex, 1);
                    }
                    this.currentTeacher = null;
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', error.data.Message);
                });
            },
            showInfo: function (messageClass, messageText) {
                this.messageText = messageText;
                this.messageClass = messageClass;
            },
            modelStateToString: function (modelState) {
                var text = '';
                for (var key in modelState) {
                    text += modelState[key].join(' ');
                }
                return text;
            },
            updateTeacher: function (teacher) {
                this.$http.get('/api/Teachers/' + teacher.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.currentTeacher.Disciplines = JSON.parse(response.data).Disciplines;
                    this.newDisciplines.splice(0, this.newDisciplines.length);
                    for (var i = 0; i < this.currentTeacher.Disciplines.length; i++) {
                        this.newDisciplines.push(this.currentTeacher.Disciplines[i].Id);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadDisciplines: function () {
                this.$http.get('/api/Disciplines',
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.disciplines.splice(0, this.disciplines.length);
                    for (var i = 0; i < response.data.length; i++) {
                        this.disciplines.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadTeachers: function () {
                this.$http.get('/api/Teachers',
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        this.teachers.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            }

        },
        computed: {
            filteredTeachers: function () {
                function filter(obj) {
                    var isFinded = true;
                    var searchText = this.searchText.toLowerCase();
                    if (obj.FullName !== null) {
                        isFinded = obj.FullName.toLowerCase().indexOf(searchText) >= 0;
                        if (isFinded) {
                            return true;
                        }
                    }
                    if (obj.Phone !== null) {
                        isFinded = obj.Phone.toLowerCase().indexOf(searchText) >= 0;
                        if (isFinded) {
                            return true;
                        }
                    }
                    return isFinded;
                }
                return this.teachers.filter(filter, this);
            }
        },
        created: function () {
            this.loadTeachers();
            this.loadDisciplines();
        }

    });

var disciplines = Vue.component('disciplines',
    {
        template: '#disciplines-template',
        data: function () {
            return {
                searchText: '',
                disciplines: [],
                teachers: [],
                newParticipants: [],
                currentDiscipline: null,
                createDiscipline: null,
                messageText: null,
                messageClass: ''
            };
        },
        methods: {
            selectDiscipline: function (discipline) {
                this.currentDiscipline = discipline;
                this.createDiscipline = null;
                this.messageText = null;
                this.updateDiscipline(discipline);
            },
            createNewDiscipline: function () {
                this.currentDiscipline = null;

                this.createDiscipline = {
                    name: ''
                };
            },
            postNewDiscipline: function () {

                var newDiscipline = {
                    name: this.createDiscipline.name
                };

                this.$http.post('/api/Disciplines', newDiscipline,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.disciplines.push(response.data);
                        this.selectDiscipline(response.data);
                        this.showInfo('alert-success', 'Дисциплина ' + response.data.Name + ' создана.');

                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    });
            },
            saveDiscipline: function () {
                var updatedDiscipline = {
                    Id: this.currentDiscipline.Id,
                    Name: this.currentDiscipline.Name,
                    TeacherIds: this.newParticipants
                }
                this.$http.put('/api/Disciplines/' + updatedDiscipline.Id, updatedDiscipline,
                        { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.showInfo('alert-success', 'Дисциплина ' + updatedDiscipline.Name + ' отредактирована.');
                    },
                    function (error) {
                        if (error.status == 401) {
                            app.$options.methods.logout();
                        }
                        this.showInfo('alert-danger', this.modelStateToString(error.data.ModelState));
                    });
            },
            removeDiscipline: function (discipline) {
                if (!confirm('Вы действительно хотите удалить дисциплину ' + discipline.Name)) {
                    return;
                }
                this.$http.delete('/api/Disciplines/' + discipline.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.showInfo('alert-warning', 'Дисциплина ' + discipline.Name + ' удалена.');
                    var removeIndex = findObjectIndexByKey(this.disciplines, 'Id', discipline.Id);
                    if (removeIndex > -1) {
                        this.disciplines.splice(removeIndex, 1);
                    }
                    this.currentDiscipline = null;
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    this.showInfo('alert-danger', error.data.Message);
                });
            },
            showInfo: function (messageClass, messageText) {
                this.messageText = messageText;
                this.messageClass = messageClass;
            },
            modelStateToString: function (modelState) {
                var text = '';
                for (var key in modelState) {
                    text += modelState[key].join(' ');
                }
                return text;
            },
            updateDiscipline: function (discipline) {
                this.$http.get('/api/Disciplines/' + discipline.Id,
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.currentDiscipline.Teachers = JSON.parse(response.data).Teachers;
                    this.newParticipants.splice(0, this.newParticipants.length);
                    for (var i = 0; i < this.currentDiscipline.Teachers.length; i++) {
                        this.newParticipants.push(this.currentDiscipline.Teachers[i].Id);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadTeachers: function () {
                this.$http.get('/api/Teachers',
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    this.teachers.splice(0, this.teachers.length);
                    for (var i = 0; i < response.data.length; i++) {
                        this.teachers.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            },
            loadDisciplines: function () {
                this.$http.get('/api/Disciplines',
                    { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                .then(
                function (response) {
                    for (var i = 0; i < response.data.length; i++) {
                        this.disciplines.push(response.data[i]);
                    }
                },
                function (error) {
                    if (error.status == 401) {
                        app.$options.methods.logout();
                    }
                    console.log(error);
                });
            }

        },
        computed: {
            filteredDisciplines: function () {
                function filter(obj) {
                    var searchText = this.searchText.toLowerCase();
                    if (obj.Name !== null) {
                        return obj.Name.toLowerCase().indexOf(searchText) >= 0;
                    }
                    return true;
                }
                return this.disciplines.filter(filter, this);
            }
        },
        created: function () {
            this.loadDisciplines();
            this.loadTeachers();
        }

    });


var routes = [
    { path: '/', component: timetable },
    { path: '/users', component: users },
    { path: '/groups', component: groups },
    { path: '/teachers', component: teachers },
    { path: '/disciplines', component: disciplines }
];

var router = new VueRouter({
    routes
    });

var app = new Vue({
        el: '#application',
    router,
        data: {
        authenticatedUser: false,
        userName: '',
        password: '',
        loginError: null,
        user: { }
    },
        methods: {
        login: function () {
                if (this.userName === '' || this.password === '') {
                    this.showLoginError('Заполните имя пользователя и пароль');
                } else {
                    var data = {
                        grant_type: 'password',
                        username: this.userName,
                        password: this.password
                    };
                    this.$http.post('/Token', data, { emulateJSON: true })
                    .then(
                        function (response) {
                            var tokenKey = response.data.token_type + ' ' + response.data.access_token;
                            sessionStorage.setItem('tokenKey', tokenKey);
                            this.getUserBaseInfo();
                        },
                        function (error) {
                            this.showLoginError(error.data.error_description);
                        }
                    );
                    this.userName = '';
                    this.password = '';
                }
        },
            logout: function () {
                this.authenticatedUser = false;
                sessionStorage.removeItem('tokenKey');
                window.location.replace('/');
            },
            showLoginError: function (text) {
                this.loginError = text;
                var context = this;
                setTimeout(function () { context.loginError = null; }, 3000);
            },
            getUserBaseInfo: function () {
                this.$http.get('/api/Account/UserBaseInfo', { headers: { Authorization: sessionStorage.getItem('tokenKey') } })
                    .then(
                    function (response) {
                        this.user = response.data;
                        this.authenticatedUser = true;                        
                    },
                    function (error) {
                        console.log(error);
                    });
            }
        },
    created: function () {
        if (sessionStorage.getItem('tokenKey') !== null) {            
            this.getUserBaseInfo();
        }
    }
});

function findObjectIndexByKey(array, key, value) {
    for (var i = 0; i < array.length; i++) {
        if (array[i][key] === value) {
            return i;
        }
    }
    return -1;
}

function findObjectByKey(array, key, value) {
    for (var i = 0; i < array.length; i++) {
        if (array[i][key] === value) {
            return array[i];
        }
    }
    return null;
}