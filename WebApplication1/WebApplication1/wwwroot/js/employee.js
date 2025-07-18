if (!$.fn.DataTable.isDataTable('#tbl1')) {
    $('#tbl1').DataTable({
        dom:
            "<'row'<'col-sm-12'f>>" +           // Üstte arama kutusu (filter)
            "<'row'<'col-sm-12'tr>>" +          // Tablo gövdesi (table)
            "<'row flex-nowrap align-items-center'<'col-6'l><'col-sm-6 d-flex justify-content-end'p>>"  // Altta: sol yarıda length menu, sağ yarıda sayfalama
    });
}
let seciliEmployeeId = null;
$(document).on("click", ".open-update-modal", function () {
    var id = $(this).data("id");
    seciliEmployeeId = id;
    console.log("Butondan gelen id:", id);

    // Session kontrolü için küçük bir AJAX çağrısı yapacağız
    $.ajax({
        url: '/Home/CheckSession', // Örnek, session kontrol endpoint'i (sen bunu backend'de yazacaksın)
        type: 'GET',
        success: function (sessionActive) {
            if (sessionActive === true) {
                // Session aktif, veri çek ve popup aç

                $.ajax({ /* JavaScript'in jQuery kütüphanesiyle AJAX (arka planda veri alma/gönderme) işlemidir.*/
                    url: '/Employee/EmployeeGetir', // Controller'a göre ayarla
                    type: 'GET',
                    data: { id: id }, /*Sunucuya gönderilecek veriler.*/
                    success: function (data) {/* AJAX isteği başarılı olursa çalışacak kod bloğu.*/
                        console.log("Modal doldurulan id:", data.Employeeid);
                        $('#Employeeid').val(data.Employeeid); /*Val değeri Input'un değerini okur (getirir)*/
                        $('#Name').val(data.Name);
                        $('#Surname').val(data.Surname);
                        $('#Employeemail').val(data.Employeemail);
                        RolleriGetir(data.Roleid);
                        // Popup göster
                        $('#Modal1').modal('show');
                    },
                });
            } else {
                // Session yok, login sayfasına yönlendir
                window.location.href = '/Login/Index';
            }
        },
        error: function () {
            // AJAX hatası (örneğin session kontrol API'sine erişim yok)
            alert('Oturum kontrolü yapılamadı. Lütfen tekrar giriş yapınız.');
            window.location.href = '/Login/Index';

        }
    });
});

$('#btnGuncelle').click(function () {
    var employee = {
        Employeeid: seciliEmployeeId,
        Name: $('#Name').val(),
        Surname: $('#Surname').val(),
        Employeemail: $('#Employeemail').val(),
        Roleid: parseInt($('#Roleid').val())
    };
    console.log("Gidecek employee id:", employee.Employeeid);

    $.ajax({
        url: '/Employee/EmployeeGuncelle',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(employee),

        success: function (response) {
            if (response.success) {
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Güncelleme başarılı!',
                    showConfirmButton: false,
                    timer: 1500,
                    timerProgressBar: true
                }).then(() => {
                    // Bildirim kapanınca sayfayı yenile
                    location.reload();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Güncelleme başarısız',
                    text: response.message
                });
            }
        },

        error: function (xhr) {
            Swal.fire({
                icon: 'error',
                title: 'Bir hata oluştu!',
                text: xhr.status + ' ' + xhr.statusText
            });
        }
    });
});

$(".btn-modal-cancel").click(function (e) {
    e.preventDefault(); // Form submit'ini engeller
    $(".modal").modal("hide");
    $('body').removeClass('modal-open');
    $('.modal-backdrop').remove();
});

// Rolleri veritabanından çekip select'e yerleştir.
//rolleri AJAX ile sunucudan çekip < select > kutusuna yerleştiriyor ve eğer önceden seçilmiş bir rol varsa onu da seçili yapıyor.
function RolleriGetir(secilenId) {
    $.ajax({
        url: '/Employee/RoleListesi',
        type: 'GET',
        success: function (roller) {
            var select = $('#Roleid');
            select.empty(); /*<select> içi tamamen temizleniyor.*/

            $.each(roller, function (i, rol) {  /*jQuery'nin each fonksiyonu ile roller dizisi içinde dönülüyor.  i: index (0, 1, 2, ...),rol: dizideki tek bir rol objesi (örneğin { Roleid: 3, RoleName: "Admin" })*/
                var seciliMi = rol.Roleid == secilenId ? "selected" : ""; /*Eğer şu anki rolun Roleid'si secilenId ile eşitse, seciliMi değişkenine "selected" stringi atanır.*/
                select.append('<option value="' + rol.Roleid + '" ' + seciliMi + '>' + rol.RoleName + '</option>');
            });
        }
    });
}
// Yeni Employee modalı için roller dropdown

$(document).on("click", ".create-update-modal", function () {
    $('#formYeniEmployee')[0].reset();
    // Önce session kontrolü yap
    $.ajax({
        url: '/Home/CheckSession',  // Backend'de yazdığın session kontrol endpoint'i
        type: 'GET',
        success: function (sessionActive) {
            if (sessionActive === true) {
                // Session aktif, rolleri çek ve modalı aç
                $.ajax({
                    url: '/Employee/RoleListesi',
                    type: 'GET',
                    headers: {
                        'Authorization': 'Bearer ' + $('#jwtToken').val()
                    },
                    success: function (roller) {
                        var select = $('#YeniRoleid');
                        select.empty();

                        $.each(roller, function (i, rol) {
                            select.append('<option value="' + rol.Roleid + '">' + rol.RoleName + '</option>');
                        });

                        // Roller yüklendikten sonra modalı göster
                        $('#ModalYeniEmployee').modal('show');
                    },
                    error: function () {
                        alert('Roller alınırken bir hata oluştu.');
                    }
                });
            } else {
                // Session yoksa login sayfasına yönlendir
                window.location.href = '/Login/Index';
            }
        },
        error: function () {
            alert('Oturum kontrolü yapılamadı. Lütfen tekrar giriş yapınız.');
            window.location.href = '/Login/Index';
        }
    });
});

// Yeni Employee kaydet butonu
$('#btnYeniKaydet').click(function () {
    if ($('#YeniPassword').val() !== $('#YeniPasswordConfirm').val()) {
        alert('Şifreler uyuşmuyor!');
        return;
    }

    var employee = {
        Name: $('#YeniName').val(),
        Surname: $('#YeniSurname').val(),
        Employeemail: $('#YeniEmployeemail').val(),
        Username: $('#YeniUsername').val(),
        Password: $('#YeniPassword').val(),
        Roleid: parseInt($('#YeniRoleid').val())
    };

    $.ajax({
        url: '/Employee/EmployeeEkle',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(employee),
        success: function (response) {
            if (response.success) {
                alert('Yeni employee başarıyla eklendi!');
                $('#ModalYeniEmployee').modal('hide');
                location.reload();
            } else {
                alert('Ekleme başarısız: ' + response.message);
            }
        },
        error: function () {
            alert('Bir hata oluştu!');
        }
    });
});

$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    if (jqXHR.status === 401) {
        Swal.fire({
            icon: 'warning',
            title: 'Oturumunuz sona erdi',
            text: 'Lütfen tekrar giriş yapınız.',
            confirmButtonText: 'Tamam'
        }).then(() => {
            window.location.href = '/Login/Index';
        });
    }
});