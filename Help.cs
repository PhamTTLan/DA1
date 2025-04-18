using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DATN
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            //Nội dung hướng dẫn sử dụng
            txtHuongDan.Text = "HƯỚNG DẪN SỬ DỤNG" + Environment.NewLine + Environment.NewLine+
                            "1. Chọn định dạng đầu vào (Text, XMI, XML) ." + Environment.NewLine +
                            "2. Chọn định dạng đầu ra (Word, Excel, HTML) " + Environment.NewLine +
                            "3. Nhấn 'Choose File' để chọn file đặc tả use case " + Environment.NewLine +
                            "4. Nhấn 'Chọn' để xác nhận chọn file " + Environment.NewLine +
                            "5. Nhấn 'Chọn' ở dòng thư mục đầu ra để lưu file test case " + Environment.NewLine +
                            "6. Nhấn 'Sinh test case' để tạo file test case " + Environment.NewLine +
                            "7. Nhấn 'Xem báo cáo' để xem độ phủ kiểm thử " + Environment.NewLine +
                            "8. Nhấn 'Làm mới để xóa các lựa chọn trước đó.";
              txtHuongDan.ReadOnly = true;
        }
        //nút Đóng
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
