using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DATN
{
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Thuộc tính cần thiết cho Windows Forms
        static void Main()
        {
            Application.EnableVisualStyles(); // Kích hoạt giao diện hiện đại
            Application.SetCompatibleTextRenderingDefault(false); // Cấu hình rendering văn bản
            //Application.Run(new Help()); // Khởi động Form1
            Application.Run(new Main());
        }
    }
}
