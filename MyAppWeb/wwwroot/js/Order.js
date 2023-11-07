var dttable;

$(document).ready(function () {
    var url = window.location.search;
    console.log(url)

    if (url.includes("pending")) {
        OrderTable("pending")
    }
     if (url.includes("approved"))
        {
            OrderTable("approved")
        }
      
            if (url.includes("shipped")) {
                OrderTable("shipped")
            }
          
                if (url.includes("underprocess"))
                {
                    OrderTable("underprocess")
                }
                else {
                    OrderTable("all")
                }
            
        
    
  
});

function OrderTable(status) {
    dttable = $('#myTable').DataTable({



        "ajax": {
            url: "/Admin/Order/AllOrders?status=" + status
        },
        "columns": [
            { "data": "name" },
            { "data": "address" },
            { "data": "city" },
            { "data": "state" },
            { "data": "phone" },

            { "data": "orderstatus" },
            { "data": "orderTotal" },
            {
                "data": "id",
                "render": function (data) {

                    return ` 
                    <a href="/Admin/Order/OrderDetails?id=${data}" > <i class="bi bi-pencil-square"></i></a >
                   `

                }
            },
        ]
    });

}