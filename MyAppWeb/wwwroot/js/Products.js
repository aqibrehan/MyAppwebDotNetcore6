var dttable;

dttable =$('#myTable').DataTable({
    "ajax": {
        url:"/Admin/Product/AllProducts"
             },
    "columns": [
        { "data": "name" },
        { "data": "description" },
        { "data": "price" },
        { "data": "imageURl" },
        { "data": "category.name" },
        {
            "data": "id",
            "render": function (data) {
          
                return ` 
                    <a href="/Admin/Product/CreateUpdate?id=${data}" > <i class="bi bi-pencil-square"></i></a >
                    <a  onclick=RemoveProduct("/Admin/Product/Delete/${data}") > <i class="bi bi-trash-fill"></i></a >`
                        
            }
        },
    ]
});


function RemoveProduct(url)
{
  
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            //Swal.fire(
            //    'Deleted!',
            //    'Your file has been deleted.',
            //    'success'
            //)
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        dttable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message)
                    }
                }
                })
        }
    })
}