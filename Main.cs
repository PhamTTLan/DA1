using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;

namespace DATN
{
    public partial class Main : Form
    {
        private List<TestCase> _testCases; // biến toàn cục để lưu danh sách test case
         
        private string _selectedFilePath; //Lưu đường dẫn file đặc tả được chọn
        private XDocument xmlDoc;  //Biến lưu trữ tài liệu XML được tải
        private string selectedUseCaseId; //Biến lưu trữ ID của use case được chọn
        public Main()
        {
            InitializeComponent();
            _testCases = new List<TestCase>(); //Khởi tạo danh sách test case

            // Gán sự kiện SelectedIndexChanged cho comboboxUC
            comboboxUC.SelectedIndexChanged += new EventHandler(ComboBoxUseCases_SelectedIndexChanged);

        }

        //lớp test case để lưu thông tin test case
        public class TestCase
        {
            //public string UseCase { get; set; }
            //public string Step { get; set; }
            //public string TestName { get; set; }
            //public string Preconditions { get; set; }
            //public string Procedure { get; set; }
            //public string ExpectedResults { get; set; }
            //public string Postconditions { get; set; }

            public string UseCase { get; set; }      // Mã Use Case (ví dụ: UC-01)
            public string UseCaseName { get; set; }  // Tên Use Case (ví dụ: Tạo/cập nhật thông tin tài khoản khách hàng)
            public string TestName { get; set; }     // Test Case ID (ví dụ: TC-01)
            public string Procedure { get; set; }    // Quy trình kiểm thử
            public string ExpectedResults { get; set; } // Kết quả kỳ vọng
        }
        public class UseCase
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        //Hàm đọc file XML 
        private bool ValidateXmlFile(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    XDocument.Parse(reader.ReadToEnd()); // Thử phân tích cú pháp XML
                }
                return true;
            }
            catch (System.Xml.XmlException xmlEx)
            {
                txtThongbao.AppendText($"File XML không hợp lệ: {xmlEx.Message}\r\n"); // Thông báo lỗi cú pháp
                txtThongbao.AppendText($"Dòng: {xmlEx.LineNumber}, Vị trí: {xmlEx.LinePosition}\r\n"); // Hiển thị dòng và vị trí
                return false;
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi kiểm tra file: {ex.Message}\r\n"); // Thông báo lỗi khác
                return false;
            }
        }

        //private void LoadXmlAndUseCases(string xmlFilePath)
        //{
        //    //try
        //    //{
        //    //    _selectedFilePath = xmlFilePath; // Lưu đường dẫn file XML
        //    //    xmlDoc = XDocument.Load(xmlFilePath); // Tải file XML

        //    //    // Lấy danh sách Use Case từ file XML và loại bỏ trùng lặp dựa trên Name
        //    //    var useCases = xmlDoc.Descendants("UseCase")
        //    //        .Where(uc => uc.Attribute("Id") != null)
        //    //        .Select(uc => new UseCase
        //    //        {
        //    //            Id = uc.Attribute("Id").Value,
        //    //            Name = uc.Attribute("Name").Value
        //    //        })
        //    //        .GroupBy(uc => uc.Name) // Nhóm theo Name để loại bỏ trùng lặp
        //    //        .Select(g => g.First()) // Chọn mục đầu tiên trong mỗi nhóm
        //    //        .OrderBy(uc => uc.Name) // Sắp xếp theo Name
        //    //        .ToList();

        //    //    // Xóa nội dung cũ trong TextBox
        //    //    txtInputTM.Clear();
        //    //    // Hiển thị toàn bộ đường dẫn thư mục chứa file XML
        //    //    string directoryPath = Path.GetDirectoryName(_selectedFilePath);
        //    //    txtInputTM.AppendText(directoryPath);

        //    //    // Hiển thị danh sách Use Case trong ComboBox
        //    //    comboboxUC.Items.Clear(); // Xóa các mục cũ
        //    //    comboboxUC.Items.AddRange(useCases.ToArray()); // Thêm danh sách use case
        //    //    comboboxUC.DisplayMember = "Name"; // Hiển thị thuộc tính Name
        //    //    comboboxUC.ValueMember = "Id"; // Lưu trữ giá trị Id

        //    //    // Đặt lại selectedUseCaseId và không chọn mặc định
        //    //    selectedUseCaseId = null;
        //    //    comboboxUC.SelectedIndex = -1; // Không chọn mục nào mặc định

        //    //    if (useCases.Count > 0)
        //    //    {
        //    //        txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
        //    //    }
        //    //    else
        //    //    {
        //    //        txtThongbao.AppendText("Không tìm thấy Use Case trong file XML!\r\n");
        //    //        selectedUseCaseId = null;
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    txtThongbao.AppendText($"Lỗi khi tải file XML: {ex.Message}\r\n");
        //    //}

        //    //try
        //    //{
        //    //    _selectedFilePath = xmlFilePath; // Lưu đường dẫn file
        //    //    xmlDoc = XDocument.Load(xmlFilePath); // Tải file

        //    //    // Danh sách Use Case
        //    //    var useCases = new List<UseCase>();

        //    //    if (radioXMLIn.Checked)
        //    //    {
        //    //        // Xử lý file XML
        //    //        useCases = xmlDoc.Descendants("UseCase")
        //    //            .Where(uc => uc.Attribute("Id") != null)
        //    //            .Select(uc => new UseCase
        //    //            {
        //    //                Id = uc.Attribute("Id").Value,
        //    //                Name = uc.Attribute("Name").Value
        //    //            })
        //    //            .GroupBy(uc => uc.Name) // Nhóm theo Name để loại bỏ trùng lặp
        //    //            .Select(g => g.First()) // Chọn mục đầu tiên trong mỗi nhóm
        //    //            .OrderBy(uc => uc.Name) // Sắp xếp theo Name
        //    //            .ToList();
        //    //    }
        //    //    else if (radioXMIIn.Checked)
        //    //    {
        //    //        // Xử lý file XMI
        //    //        useCases = xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}packagedElement")
        //    //            .Where(uc => uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}type")?.Value == "uml:UseCase")
        //    //            .Select(uc => new UseCase
        //    //            {
        //    //                Id = uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value,
        //    //                Name = uc.Attribute("name")?.Value
        //    //            })
        //    //            .GroupBy(uc => uc.Name) // Nhóm theo Name để loại bỏ trùng lặp
        //    //            .Select(g => g.First()) // Chọn mục đầu tiên trong mỗi nhóm
        //    //            .OrderBy(uc => uc.Name) // Sắp xếp theo Name
        //    //            .ToList();
        //    //    }

        //    //    // Xóa nội dung cũ trong TextBox
        //    //    txtInputTM.Clear();
        //    //    // Hiển thị toàn bộ đường dẫn thư mục chứa file
        //    //    string directoryPath = Path.GetDirectoryName(_selectedFilePath);
        //    //    txtInputTM.AppendText(directoryPath);

        //    //    // Hiển thị danh sách Use Case trong ComboBox
        //    //    comboboxUC.Items.Clear(); // Xóa các mục cũ
        //    //    comboboxUC.Items.AddRange(useCases.ToArray()); // Thêm danh sách use case
        //    //    comboboxUC.DisplayMember = "Name"; // Hiển thị thuộc tính Name
        //    //    comboboxUC.ValueMember = "Id"; // Lưu trữ giá trị Id

        //    //    // Đặt lại selectedUseCaseId và không chọn mặc định
        //    //    selectedUseCaseId = null;
        //    //    comboboxUC.SelectedIndex = -1; // Không chọn mục nào mặc định

        //    //    if (useCases.Count > 0)
        //    //    {
        //    //        txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
        //    //    }
        //    //    else
        //    //    {
        //    //        txtThongbao.AppendText($"Không tìm thấy Use Case trong file {(radioXMLIn.Checked ? "XML" : "XMI")}!\r\n");
        //    //        selectedUseCaseId = null;
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    txtThongbao.AppendText($"Lỗi khi tải file {(radioXMLIn.Checked ? "XML" : "XMI")}: {ex.Message}\r\n");
        //    //}


        //    try
        //    {
        //        _selectedFilePath = xmlFilePath; // Lưu đường dẫn file
        //        xmlDoc = XDocument.Load(xmlFilePath); // Tải file

        //        // Danh sách Use Case
        //        var useCases = new List<UseCase>();

        //        if (radioXMLIn.Checked)
        //        {
        //            // Xử lý file XML
        //            useCases = xmlDoc.Descendants("UseCase")
        //                .Where(uc => uc.Attribute("Id") != null)
        //                .Select(uc => new UseCase
        //                {
        //                    Id = uc.Attribute("Id").Value,
        //                    Name = uc.Attribute("Name").Value
        //                })
        //                .GroupBy(uc => uc.Name) // Nhóm theo Name để loại bỏ trùng lặp
        //                .Select(g => g.First()) // Chọn mục đầu tiên trong mỗi nhóm
        //                .OrderBy(uc => uc.Name) // Sắp xếp theo Name
        //                .ToList();
        //        }
        //        else if (radioXMIIn.Checked)
        //        {
        //            // Xử lý file XMI
        //            useCases = xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}packagedElement")
        //                .Where(uc => uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}type")?.Value == "uml:UseCase")
        //                .Select(uc => new UseCase
        //                {
        //                    Id = uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value ?? "",
        //                    Name = uc.Attribute("name")?.Value ?? "Unnamed Use Case"
        //                })
        //                .Where(uc => !string.IsNullOrEmpty(uc.Id)) // Đảm bảo Id không rỗng
        //                .OrderBy(uc => uc.Name) // Sắp xếp theo Name
        //                .ToList();

        //            // Debug: Hiển thị danh sách Use Cases tìm thấy
        //            if (useCases.Any())
        //            {
        //                txtThongbao.AppendText("Danh sách Use Cases tìm thấy trong file XMI:\r\n");
        //                foreach (var uc in useCases)
        //                {
        //                    txtThongbao.AppendText($"ID: {uc.Id}, Name: {uc.Name}\r\n");
        //                }
        //            }
        //        }

        //        // Xóa nội dung cũ trong TextBox
        //        txtInputTM.Clear();
        //        // Hiển thị toàn bộ đường dẫn thư mục chứa file
        //        string directoryPath = Path.GetDirectoryName(_selectedFilePath);
        //        txtInputTM.AppendText(directoryPath);

        //        // Hiển thị danh sách Use Case trong ComboBox
        //        comboboxUC.Items.Clear(); // Xóa các mục cũ
        //        comboboxUC.Items.AddRange(useCases.ToArray()); // Thêm danh sách use case
        //        comboboxUC.DisplayMember = "Name"; // Hiển thị thuộc tính Name
        //        comboboxUC.ValueMember = "Id"; // Lưu trữ giá trị Id

        //        // Đặt lại selectedUseCaseId và không chọn mặc định
        //        selectedUseCaseId = null;
        //        comboboxUC.SelectedIndex = -1; // Không chọn mục nào mặc định

        //        if (useCases.Count > 0)
        //        {
        //            txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
        //        }
        //        else
        //        {
        //            txtThongbao.AppendText($"Không tìm thấy Use Case trong file {(radioXMLIn.Checked ? "XML" : "XMI")}!\r\n");
        //            selectedUseCaseId = null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        txtThongbao.AppendText($"Lỗi khi tải file {(radioXMLIn.Checked ? "XML" : "XMI")}: {ex.Message}\r\n");
        //    }
        //}

        private void LoadXmlAndUseCases(string xmlFilePath)
        {
            try
            {
                _selectedFilePath = xmlFilePath; // Lưu đường dẫn file
                xmlDoc = XDocument.Load(xmlFilePath); // Tải file

                // Danh sách Use Case
                var useCases = new List<UseCase>();

                if (radioXMLIn.Checked)
                { 
                    // Xử lý file XML
                    useCases = xmlDoc.Descendants("UseCase")
                        .Where(uc => uc.Attribute("Id") != null)
                        .Select(uc => new UseCase
                        {
                            Id = uc.Attribute("Id").Value,
                            Name = uc.Attribute("Name").Value
                        })
                        .GroupBy(uc => uc.Name) // Nhóm theo Name để loại bỏ trùng lặp
                        .Select(g => g.First()) // Chọn mục đầu tiên trong mỗi nhóm
                        .OrderBy(uc => uc.Name) // Sắp xếp theo Name
                        .ToList();
                }
                else if (radioXMIIn.Checked)
                {
                    // Xử lý file XMI
                    // Thử tìm Use Case trong thẻ <packagedElement> với type='uml:UseCase'
                    useCases = xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}packagedElement")
                        .Where(uc => uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}type")?.Value == "uml:UseCase")
                        .Select(uc => new UseCase
                        {
                            Id = uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value ?? uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}idref")?.Value ?? "temp_id_" + Guid.NewGuid().ToString(),
                            Name = uc.Attribute("name")?.Value ?? "Unnamed Use Case"
                        })
                        .OrderBy(uc => uc.Name)
                        .ToList();

                    // Nếu không tìm thấy, thử tìm trong thẻ <element>
                    if (!useCases.Any())
                    {
                        txtThongbao.AppendText("Không tìm thấy Use Case trong thẻ <packagedElement>. Đang thử tìm trong thẻ <element>...\r\n");
                        useCases = xmlDoc.Descendants("element")
                            .Where(el => el.Attribute("xmi:type")?.Value == "uml:UseCase" || el.Attribute("name")?.Value.ToLower().Contains("use case") == true)
                            .Select(el => new UseCase
                            {
                                Id = el.Attribute("xmi:id")?.Value ?? el.Attribute("xmi:idref")?.Value ?? "temp_id_" + Guid.NewGuid().ToString(),
                                Name = el.Attribute("name")?.Value ?? "Unnamed Use Case"
                            })
                            .OrderBy(uc => uc.Name)
                            .ToList();
                    }

                    // Nếu vẫn không tìm thấy, thử tìm trong thẻ <ownedMember>
                    if (!useCases.Any())
                    {
                        txtThongbao.AppendText("Không tìm thấy Use Case trong thẻ <element>. Đang thử tìm trong thẻ <ownedMember>...\r\n");
                        useCases = xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}ownedMember")
                            .Where(om => om.Attribute("{http://schema.omg.org/spec/XMI/2.1}type")?.Value == "uml:UseCase")
                            .Select(om => new UseCase
                            {
                                Id = om.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value ?? om.Attribute("{http://schema.omg.org/spec/XMI/2.1}idref")?.Value ?? "temp_id_" + Guid.NewGuid().ToString(),
                                Name = om.Attribute("name")?.Value ?? "Unnamed Use Case"
                            })
                            .OrderBy(uc => uc.Name)
                            .ToList();
                    }

                    // Nếu vẫn không tìm thấy, thử tìm trong thẻ <uml:UseCase>
                    if (!useCases.Any())
                    {
                        txtThongbao.AppendText("Không tìm thấy Use Case trong thẻ <ownedMember>. Đang thử tìm trong thẻ <uml:UseCase>...\r\n");
                        useCases = xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}UseCase")
                            .Select(uc => new UseCase
                            {
                                Id = uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value ?? uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}idref")?.Value ?? "temp_id_" + Guid.NewGuid().ToString(),
                                Name = uc.Attribute("name")?.Value ?? "Unnamed Use Case"
                            })
                            .OrderBy(uc => uc.Name)
                            .ToList();
                    }

                    // Nếu vẫn không tìm thấy, thử tìm trong thẻ <vpumlModel>
                    if (!useCases.Any())
                    {
                        txtThongbao.AppendText("Không tìm thấy Use Case trong thẻ <uml:UseCase>. Đang thử tìm trong thẻ <vpumlModel>...\r\n");
                        useCases = xmlDoc.Descendants("vpumlModel")
                            .Where(vm => vm.Attribute("modelType")?.Value == "UseCase")
                            .Select(vm => new UseCase
                            {
                                Id = vm.Attribute("id")?.Value ?? vm.Attribute("xmi:id")?.Value ?? vm.Attribute("xmi:idref")?.Value ?? "temp_id_" + Guid.NewGuid().ToString(),
                                Name = vm.Element("properties")?.Elements("property")
                                    .FirstOrDefault(p => p.Attribute("name")?.Value == "name")?.Attribute("value")?.Value ?? "Unnamed Use Case"
                            })
                            .OrderBy(uc => uc.Name)
                            .ToList();
                    }

                    // Debug chi tiết: Hiển thị thông tin về file XMI
                    txtThongbao.AppendText("Thông tin debug về file XMI:\r\n");
                    txtThongbao.AppendText($"Số lượng thẻ <packagedElement>: {xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}packagedElement").Count()}\r\n");
                    txtThongbao.AppendText($"Số lượng thẻ <element>: {xmlDoc.Descendants("element").Count()}\r\n");
                    txtThongbao.AppendText($"Số lượng thẻ <ownedMember>: {xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}ownedMember").Count()}\r\n");
                    txtThongbao.AppendText($"Số lượng thẻ <uml:UseCase>: {xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}UseCase").Count()}\r\n");
                    txtThongbao.AppendText($"Số lượng thẻ <vpumlModel>: {xmlDoc.Descendants("vpumlModel").Count()}\r\n");

                    // Debug: Hiển thị các modelType của thẻ <vpumlModel>
                    var modelTypes = xmlDoc.Descendants("vpumlModel")
                        .Select(vm => vm.Attribute("modelType")?.Value)
                        .Distinct()
                        .Where(mt => mt != null);
                    if (modelTypes.Any())
                    {
                        txtThongbao.AppendText("Các giá trị modelType trong thẻ <vpumlModel>:\r\n");
                        foreach (var mt in modelTypes)
                        {
                            txtThongbao.AppendText($"- {mt}\r\n");
                        }
                    }
                    else
                    {
                        txtThongbao.AppendText("Không tìm thấy thẻ <vpumlModel> nào có thuộc tính modelType.\r\n");
                    }

                    // Debug: Hiển thị nội dung của một số thẻ tiềm năng (nếu có)
                    var potentialElements = xmlDoc.Descendants()
                        .Where(el => el.Name.LocalName == "packagedElement" || el.Name.LocalName == "element" || el.Name.LocalName == "vpumlModel")
                        .Take(3); // Lấy tối đa 3 thẻ để tránh thông báo quá dài
                    if (potentialElements.Any())
                    {
                        txtThongbao.AppendText("Mẫu nội dung của các thẻ tiềm năng (tối đa 3 thẻ):\r\n");
                        foreach (var el in potentialElements)
                        {
                            txtThongbao.AppendText($"Thẻ: {el.Name.LocalName}\r\n");
                            txtThongbao.AppendText($"Nội dung: {el.ToString().Substring(0, Math.Min(200, el.ToString().Length))}...\r\n");
                            txtThongbao.AppendText("----\r\n");
                        }
                    }

                    // Debug: Hiển thị danh sách Use Cases tìm thấy
                    if (useCases.Any())
                    {
                        txtThongbao.AppendText("Danh sách Use Cases tìm thấy trong file XMI:\r\n");
                        foreach (var uc in useCases)
                        {
                            txtThongbao.AppendText($"ID: {uc.Id}, Name: {uc.Name}\r\n");
                        }
                    }
                    else
                    {
                        txtThongbao.AppendText("Không tìm thấy Use Case nào trong file XMI!\r\n");
                        txtThongbao.AppendText("Vui lòng kiểm tra nội dung file XMI:\r\n");
                        txtThongbao.AppendText("- Đảm bảo file có chứa các Use Case trong thẻ <packagedElement>, <element>, <ownedMember>, <uml:UseCase>, hoặc <vpumlModel> với modelType='UseCase'.\r\n");
                        txtThongbao.AppendText("- Đảm bảo các Use Case có thuộc tính 'id' (hoặc 'xmi:id', 'xmi:idref') và 'name' hợp lệ (không rỗng).\r\n");
                        txtThongbao.AppendText("- Mở file XMI bằng trình chỉnh sửa XML (như Notepad++, VS Code) và kiểm tra các thẻ trên.\r\n");
                        txtThongbao.AppendText("- Nếu file không đúng định dạng, hãy xuất lại file XMI từ công cụ mô hình (Visual Paradigm, Enterprise Architect, v.v.).\r\n");
                        txtThongbao.AppendText("- Đảm bảo file tuân theo chuẩn UML 2.0 và chứa Use Case Diagram.\r\n");
                        txtThongbao.AppendText("- Kiểm tra xem file có chứa các thành phần khác (như Class Diagram, Activity Diagram) thay vì Use Case Diagram.\r\n");
                    }
                }

                // Xóa nội dung cũ trong TextBox
                txtInputTM.Clear();
                // Hiển thị toàn bộ đường dẫn thư mục chứa file
                string directoryPath = Path.GetDirectoryName(_selectedFilePath);
                txtInputTM.AppendText(directoryPath);

                // Hiển thị danh sách Use Case trong ComboBox
                comboboxUC.Items.Clear(); // Xóa các mục cũ
                comboboxUC.Items.AddRange(useCases.ToArray()); // Thêm danh sách use case
                comboboxUC.DisplayMember = "Name"; // Hiển thị thuộc tính Name
                comboboxUC.ValueMember = "Id"; // Lưu trữ giá trị Id

                // Đặt lại selectedUseCaseId và không chọn mặc định
                selectedUseCaseId = null;
                comboboxUC.SelectedIndex = -1; // Không chọn mục nào mặc định

                if (useCases.Count > 0)
                {
                    txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
                }
                else
                {
                    txtThongbao.AppendText($"Không tìm thấy Use Case trong file {(radioXMLIn.Checked ? "XML" : "XMI")}!\r\n");
                    txtThongbao.AppendText("Vui lòng kiểm tra nội dung file:\r\n");
                    txtThongbao.AppendText("- Đảm bảo file có định dạng đúng (tuân theo chuẩn UML 2.0 nếu là file XMI).\r\n");
                    txtThongbao.AppendText("- Đảm bảo các Use Case có thuộc tính 'id' (hoặc 'xmi:id', 'xmi:idref') và 'name' hợp lệ (không rỗng).\r\n");
                    txtThongbao.AppendText("- Mở file XMI bằng trình chỉnh sửa XML (như Notepad++, VS Code) và kiểm tra các thẻ liên quan.\r\n");
                    txtThongbao.AppendText("- Nếu file không đúng định dạng, hãy xuất lại file XMI từ công cụ mô hình (Visual Paradigm, Enterprise Architect, v.v.).\r\n");
                    txtThongbao.AppendText("- Đảm bảo file chứa Use Case Diagram và tuân theo chuẩn UML 2.0.\r\n");
                    txtThongbao.AppendText("- Kiểm tra xem file có chứa các thành phần khác (như Class Diagram, Activity Diagram) thay vì Use Case Diagram.\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi tải file {(radioXMLIn.Checked ? "XML" : "XMI")}:\r\n");
                txtThongbao.AppendText($"Lỗi: {ex.Message}\r\n");
                txtThongbao.AppendText($"Stack Trace: {ex.StackTrace}\r\n");
                txtThongbao.AppendText("Vui lòng kiểm tra nội dung file hoặc liên hệ hỗ trợ.\r\n");
            }
        }


        //Hàm đọc và phân tích file .txt
        private bool ValidateTxtFile(string filePath)
        {
         
            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);

                // Kiểm tra các thành phần bắt buộc, không phân biệt hoa thường
                bool hasUseCaseId = Regex.IsMatch(content, @"Use case ID\s*:\s*.+", RegexOptions.IgnoreCase);
                bool hasUseCaseName = Regex.IsMatch(content, @"Use case name\s*:\s*.+", RegexOptions.IgnoreCase);
                bool hasMainFlow = Regex.IsMatch(content, @"(Main Flow|BasicFlow|Primary Flow)\s*:[\s\S]*", RegexOptions.IgnoreCase);

                if (!hasUseCaseId)
                {
                    txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Use case ID'!\r\n");
                    return false;
                }
                if (!hasUseCaseName)
                {
                    txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Use case name'!\r\n");
                    return false;
                }
                if (!hasMainFlow)
                {
                    txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Main Flow', 'BasicFlow' hoặc 'Primary Flow'!\r\n");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi kiểm tra file .txt: {ex.Message}\r\n");
                return false;
            }
        }

        private string ExtractValue(string content, string pattern, int groupIndex = 1, RegexOptions options = RegexOptions.None)
        {
            var match = Regex.Match(content, pattern, options);
            if (match.Success && match.Groups.Count > groupIndex)
            {
                return match.Groups[groupIndex].Value.Trim();
            }
            return string.Empty;
        }

        private void ParseTxtFile(string txtFilePath)
        {

            //try
            //{
            //    _testCases.Clear(); // Xóa danh sách test case cũ
            //    txtThongbao.Text = string.Empty; // Xóa hoàn toàn nội dung thông báo

            //    // Đọc toàn bộ nội dung file .txt
            //    string content = File.ReadAllText(txtFilePath, Encoding.UTF8);

            //    // Lấy thông tin Use Case
            //    string useCaseId = ExtractValue(content, @"Use case ID\s*:\s*(.*?)(?:\n|$)", 1, RegexOptions.IgnoreCase);
            //    if (string.IsNullOrEmpty(useCaseId))
            //    {
            //        txtThongbao.Text = "Không tìm thấy 'Use case ID' trong file .txt!\r\n";
            //        return;
            //    }

            //    string useCaseName = ExtractValue(content, @"Use case name\s*:\s*(.*?)(?:\n|$)", 1, RegexOptions.IgnoreCase);
            //    if (string.IsNullOrEmpty(useCaseName))
            //    {
            //        txtThongbao.Text = "Không tìm thấy 'Use case name' trong file .txt!\r\n";
            //        return;
            //    }

            //    // Hỗ trợ cả Preconditions và Pre-Condition(s)
            //    string preconditions = ExtractValue(content, @"(Pre-Condition\(s\)|Preconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|Post-Condition\(s\)|Postconditions|$))", 2, RegexOptions.IgnoreCase);

            //    // Hỗ trợ cả Postconditions và Post-Condition(s)
            //    string postconditions = ExtractValue(content, @"(Post-Condition\(s\)|Postconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|$))", 2, RegexOptions.IgnoreCase);

            //    // UseCase lấy từ useCaseId (ví dụ: UC_001 hoặc UC-1.1)
            //    string useCase = string.IsNullOrEmpty(useCaseId) ? "UC-Unknown" : useCaseId;

            //    // Sinh test case cho BasicFlow hoặc Main Flow
            //    string basicFlowPattern = @"(BasicFlow|Main Flow|Primary Flow)\s*:([\s\S]*?)(?=(?:ExceptionFlow|Alternative Flows|Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))";
            //    string basicFlow = ExtractValue(content, basicFlowPattern, 2, RegexOptions.IgnoreCase);
            //    if (!string.IsNullOrEmpty(basicFlow))
            //    {
            //        var testCase = new TestCase
            //        {
            //            UseCase = useCase,
            //            Step = "Main Flow",
            //            TestName = "TC-01",
            //            Preconditions = preconditions,
            //            Procedure = FormatSteps(basicFlow).Replace("Đăng ký", "Đăng nhập"),
            //            ExpectedResults = "Hệ thống xác thực thông tin đăng nhập thành công và chuyển đến trang chủ",
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase);
            //    }

            //    // Sinh test case cho ExceptionFlow hoặc Alternative Flows
            //    string exceptionFlowPattern = @"(ExceptionFlow|Alternative Flows)\s*:([\s\S]*?)(?=(?:Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))";
            //    string exceptionFlow = ExtractValue(content, exceptionFlowPattern, 2, RegexOptions.IgnoreCase);
            //    if (!string.IsNullOrEmpty(exceptionFlow))
            //    {
            //        // Test case cho ExceptionFlow/Alternative Flow A1 (Đăng nhập không thành công)
            //        var testCase1 = new TestCase
            //        {
            //            UseCase = useCase,
            //            Step = "ExceptionFlow A1",
            //            TestName = "TC-02",
            //            Preconditions = preconditions,
            //            Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'"),
            //            ExpectedResults = "Hệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'",
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase1);

            //        // Test case cho ExceptionFlow/Alternative Flow A1.1 (Thử lại)
            //        var testCase2 = new TestCase
            //        {
            //            UseCase = useCase,
            //            Step = "ExceptionFlow A1.1",
            //            TestName = "TC-03",
            //            Preconditions = preconditions,
            //            Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'\nNgười dùng thử lại"),
            //            ExpectedResults = "Người dùng quay lại bước nhập thông tin đăng nhập",
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase2);

            //        // Test case cho ExceptionFlow/Alternative Flow A1.2 (Quên mật khẩu)
            //        var testCase3 = new TestCase
            //        {
            //            UseCase = useCase,
            //            Step = "ExceptionFlow A1.2",
            //            TestName = "TC-04",
            //            Preconditions = preconditions,
            //            Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'\nNgười dùng chọn 'Quên mật khẩu'"),
            //            ExpectedResults = "Hệ thống chuyển đến giao diện khôi phục mật khẩu",
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase3);
            //    }

            //    // Sinh test case cho Exceptions (nếu có)
            //    string exceptionsPattern = @"Exceptions\s*:([\s\S]*?)(?=(?:Post-Condition\(s\)|Postconditions|$))";
            //    string exceptions = ExtractValue(content, exceptionsPattern, 1, RegexOptions.IgnoreCase);
            //    if (!string.IsNullOrEmpty(exceptions))
            //    {
            //        // Test case cho Exception E1 (Tên đăng nhập hoặc email không tồn tại)
            //        if (exceptions.Contains("E1"))
            //        {
            //            var testCase4 = new TestCase
            //            {
            //                UseCase = useCase,
            //                Step = "Exception E1",
            //                TestName = "TC-05",
            //                Preconditions = preconditions,
            //                Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc email không tồn tại"),
            //                ExpectedResults = "Hệ thống hiển thị thông báo 'Tên đăng nhập hoặc email không tồn tại trong hệ thống'",
            //                Postconditions = postconditions
            //            };
            //            _testCases.Add(testCase4);
            //        }

            //        // Test case cho Exception E2 (Lỗi gửi email)
            //        if (exceptions.Contains("E2"))
            //        {
            //            var testCase5 = new TestCase
            //            {
            //                UseCase = useCase,
            //                Step = "Exception E2",
            //                TestName = "TC-06",
            //                Preconditions = preconditions,
            //                Procedure = FormatSteps("Hệ thống gặp lỗi khi gửi email khôi phục"),
            //                ExpectedResults = "Hệ thống hiển thị thông báo 'Không thể gửi email khôi phục, vui lòng thử lại sau'",
            //                Postconditions = postconditions
            //            };
            //            _testCases.Add(testCase5);
            //        }
            //    }

            //    // Hiển thị thông báo khi sinh test case thành công hoặc không sinh được
            //    txtThongbao.Text = _testCases.Count > 0
            //        ? $"Đã sinh ra {_testCases.Count} test case\r\n"
            //        : "Không sinh được test case nào từ file .txt!\r\n";
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.Text = $"Lỗi khi phân tích file .txt: {ex.Message}\r\n";
            //}

            try
            {
                _testCases.Clear();
                txtThongbao.Text = string.Empty;

                string content = File.ReadAllText(txtFilePath, Encoding.UTF8);

                string useCaseId = ExtractValue(content, @"Use case ID\s*:\s*(.*?)(?:\n|$)", 1, RegexOptions.IgnoreCase);
                if (string.IsNullOrEmpty(useCaseId))
                {
                    txtThongbao.Text = "Không tìm thấy 'Use case ID' trong file .txt!\r\n";
                    return;
                }

                string useCaseName = ExtractValue(content, @"Use case name\s*:\s*(.*?)(?:\n|$)", 1, RegexOptions.IgnoreCase);
                if (string.IsNullOrEmpty(useCaseName))
                {
                    txtThongbao.Text = "Không tìm thấy 'Use case name' trong file .txt!\r\n";
                    return;
                }

                string preconditions = ExtractValue(content, @"(Pre-Condition\(s\)|Preconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|Post-Condition\(s\)|Postconditions|$))", 2, RegexOptions.IgnoreCase);
                string postconditions = ExtractValue(content, @"(Post-Condition\(s\)|Postconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|$))", 2, RegexOptions.IgnoreCase);

                string useCase = string.IsNullOrEmpty(useCaseId) ? "UC-Unknown" : useCaseId;

                string basicFlowPattern = @"(BasicFlow|Main Flow|Primary Flow)\s*:([\s\S]*?)(?=(?:ExceptionFlow|Alternative Flows|Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))";
                string basicFlow = ExtractValue(content, basicFlowPattern, 2, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(basicFlow))
                {
                    var testCase = new TestCase
                    {
                        UseCase = useCase,
                        UseCaseName = useCaseName,
                        TestName = "TC-01",
                        Procedure = "Kiểm tra luồng chính:\n" + FormatSteps(basicFlow).Replace("Đăng ký", "Đăng nhập"),
                        ExpectedResults = postconditions
                    };
                    _testCases.Add(testCase);
                }

                string exceptionFlowPattern = @"(ExceptionFlow|Alternative Flows)\s*:([\s\S]*?)(?=(?:Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))";
                string exceptionFlow = ExtractValue(content, exceptionFlowPattern, 2, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(exceptionFlow))
                {
                    var testCase1 = new TestCase
                    {
                        UseCase = useCase,
                        UseCaseName = useCaseName,
                        TestName = "TC-02",
                        Procedure = "Kiểm tra trường hợp Đề nghị dữ liệu không đầy đủ ở bước 1:\n" + FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'"),
                        ExpectedResults = "Hệ thống thông báo lỗi: Đề nghị dữ liệu không đầy đủ"
                    };
                    _testCases.Add(testCase1);

                    var testCase2 = new TestCase
                    {
                        UseCase = useCase,
                        UseCaseName = useCaseName,
                        TestName = "TC-03",
                        Procedure = "Kiểm tra trường hợp Đề nghị dữ liệu không đầy đủ ở bước 2:\n" + FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'\nNgười dùng thử lại"),
                        ExpectedResults = "Hệ thống thông báo lỗi: Đề nghị dữ liệu không đầy đủ"
                    };
                    _testCases.Add(testCase2);

                    var testCase3 = new TestCase
                    {
                        UseCase = useCase,
                        UseCaseName = useCaseName,
                        TestName = "TC-04",
                        Procedure = "Kiểm tra trường hợp Thông tin không hợp lệ:\n" + FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'\nNgười dùng chọn 'Quên mật khẩu'"),
                        ExpectedResults = "Hệ thống thông báo lỗi: Thông tin không hợp lệ"
                    };
                    _testCases.Add(testCase3);
                }

                txtThongbao.Text = _testCases.Count > 0
                    ? $"Đã sinh ra {_testCases.Count} test case\r\n"
                    : "Không sinh được test case nào từ file .txt!\r\n";
            }
            catch (Exception ex)
            {
                txtThongbao.Text = $"Lỗi khi phân tích file .txt: {ex.Message}\r\n";
            }

        }

        // Hàm hỗ trợ trích xuất giá trị từ nội dung file .txt bằng regex
        private string ExtractValue(string content, string pattern, int groupIndex = 1)
        {
            var match = Regex.Match(content, pattern);
            if (match.Success && match.Groups.Count > groupIndex)
            {
                return match.Groups[groupIndex].Value.Trim();
            }
            return string.Empty;


        }

        // Hàm định dạng các bước thành chuỗi
        private string FormatSteps(string steps)
        {
            // Loại bỏ các dòng trống và định dạng các bước
            var stepLines = steps.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => line.Trim())
                                .Where(line => !string.IsNullOrWhiteSpace(line));
            return string.Join("\n", stepLines);
        }



        private void btnInput_Click(object sender, EventArgs e)
        {
            //using (var openFileDialog = new OpenFileDialog())
            //{
            //    // Thiết lập thư mục mặc định
            //    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //    openFileDialog.Title = "Chọn file đặc tả";
            //    openFileDialog.RestoreDirectory = true; // Khôi phục thư mục đã chọn trước đó

            //    // Kiểm tra định dạng đầu vào đã chọn
            //    if (radioTextIn.Checked)
            //    {
            //        openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            //    }
            //    else if (radioXMLIn.Checked)
            //    {
            //        openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            //    }
            //    else if (radioXMIIn.Checked)
            //    {
            //        openFileDialog.Filter = "XMI files (*.xmi)|*.xmi|All files (*.*)|*.*";
            //    }
            //    else
            //    {
            //        openFileDialog.Filter = "Supported files (*.txt;*.xml;*.xmi)|*.txt;*.xml;*.xmi|All files (*.*)|*.*";
            //    }

            //    if (openFileDialog.ShowDialog() == DialogResult.OK)
            //    {
            //        string fullPath = openFileDialog.FileName;
            //        if (radioTextIn.Checked)
            //        {
            //            // Kiểm tra file .txt có hợp lệ không
            //            if (!ValidateTxtFile(fullPath))
            //            {
            //                _selectedFilePath = null; // Đặt lại nếu file không hợp lệ
            //                txtInputTM.Clear();
            //                return;
            //            }

            //            // Lưu đường dẫn file và hiển thị đường dẫn đầy đủ
            //            _selectedFilePath = fullPath;
            //            txtInputTM.Text = fullPath; // Hiển thị đường dẫn đầy đủ của file
            //        }
            //        else if (radioXMLIn.Checked)
            //        {
            //            // Kiểm tra file XML có hợp lệ không
            //            if (!ValidateXmlFile(fullPath))
            //            {
            //                return;
            //            }
            //            LoadXmlAndUseCases(fullPath); // Tải danh sách Use Case từ file XML
            //        }
            //        else if (radioXMIIn.Checked)
            //        {
            //            // Xử lý file XMI (chưa triển khai)
            //            txtThongbao.AppendText("Chức năng xử lý file XMI chưa được triển khai.\r\n");
            //        }
            //    }
            //}

            using (var openFileDialog = new OpenFileDialog())
            {
                // Thiết lập thư mục mặc định
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openFileDialog.Title = "Chọn file đặc tả";
                openFileDialog.RestoreDirectory = true; // Khôi phục thư mục đã chọn trước đó

                // Kiểm tra định dạng đầu vào đã chọn
                if (radioTextIn.Checked)
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                }
                else if (radioXMLIn.Checked)
                {
                    openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                }
                else if (radioXMIIn.Checked)
                {
                    openFileDialog.Filter = "XMI files (*.xmi;*.uml)|*.xmi;*.uml|All files (*.*)|*.*";
                }
                else
                {
                    openFileDialog.Filter = "Supported files (*.txt;*.xml;*.xmi;*.uml)|*.txt;*.xml;*.xmi;*.uml|All files (*.*)|*.*";
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fullPath = openFileDialog.FileName;
                    if (radioTextIn.Checked)
                    {
                        // Kiểm tra file .txt có hợp lệ không
                        if (!ValidateTxtFile(fullPath))
                        {
                            _selectedFilePath = null; // Đặt lại nếu file không hợp lệ
                            txtInputTM.Clear();
                            return;
                        }

                        // Lưu đường dẫn file và hiển thị đường dẫn đầy đủ
                        _selectedFilePath = fullPath;
                        txtInputTM.Text = fullPath; // Hiển thị đường dẫn đầy đủ của file
                    }
                    else if (radioXMLIn.Checked)
                    {
                        // Kiểm tra file XML có hợp lệ không
                        if (!ValidateXmlFile(fullPath))
                        {
                            return;
                        }
                        LoadXmlAndUseCases(fullPath); // Tải danh sách Use Case từ file XML
                    }
                    else if (radioXMIIn.Checked)
                    {
                        // Kiểm tra file XMI có hợp lệ không
                        if (!ValidateXmiFile(fullPath))
                        {
                            return;
                        }
                        LoadXmlAndUseCases(fullPath); // Tải danh sách Use Case từ file XMI
                    }
                }
            }

        }

        private void btnOutput_Click(object sender, EventArgs e)
        {

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Select Folder|SelectFolder.txt"; // Bộ lọc giả để chọn thư mục
                openFileDialog.Title = "Chọn thư mục để lưu báo cáo";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openFileDialog.CheckFileExists = false; // Tắt kiểm tra file tồn tại
                openFileDialog.CheckPathExists = true; // Đảm bảo đường dẫn thư mục tồn tại
                openFileDialog.FileName = "SelectFolder.txt"; // Tên file giả để hiển thị trong hộp thoại

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = openFileDialog.FileName; // Đường dẫn chứa file giả
                    string selectedFolder = Path.GetDirectoryName(selectedPath); // Lấy đường dẫn thư mục
                    txtOutputTM.Text = selectedFolder; // Hiển thị toàn bộ đường dẫn thư mục
                }
            }
        }

        //đảm bảo người dùng chọn 1 cá sử dụng trong combobox
        private void ComboBoxUseCases_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if (comboboxUC.SelectedIndex > -1)
            //{
            //    var selectedUseCase = comboboxUC.SelectedItem as UseCase;
            //    if (selectedUseCase != null && !string.IsNullOrEmpty(selectedUseCase.Id))
            //    {
            //        selectedUseCaseId = selectedUseCase.Id;
            //        txtThongbao.Clear();
            //        txtThongbao.AppendText($"Đã chọn Use Case: {selectedUseCase.Name}, ID: {selectedUseCaseId}\r\n");
            //    }
            //    else
            //    {
            //        selectedUseCaseId = null;
            //        txtThongbao.Clear();
            //        txtThongbao.AppendText("Không lấy được ID của Use Case!\r\n");
            //    }
            //}
            //else
            //{
            //    selectedUseCaseId = null;
            //    txtThongbao.Clear();
            //    txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
            //}

            if (comboboxUC.SelectedIndex > -1)
            {
                var selectedUseCase = comboboxUC.SelectedItem as UseCase;
                if (selectedUseCase != null)
                {
                    if (!string.IsNullOrEmpty(selectedUseCase.Id))
                    {
                        selectedUseCaseId = selectedUseCase.Id;
                        txtThongbao.Clear();
                        txtThongbao.AppendText($"Đã chọn Use Case: {selectedUseCase.Name}, ID: {selectedUseCaseId}\r\n");
                    }
                    else
                    {
                        selectedUseCaseId = null;
                        txtThongbao.Clear();
                        txtThongbao.AppendText($"Lỗi: Use Case '{selectedUseCase.Name}' không có ID hợp lệ!\r\n");
                    }
                }
                else
                {
                    selectedUseCaseId = null;
                    txtThongbao.Clear();
                    txtThongbao.AppendText("Lỗi: Không thể lấy thông tin Use Case từ lựa chọn!\r\n");
                }
            }
            else
            {
                selectedUseCaseId = null;
                txtThongbao.Clear();
                txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
            }
        }


        //private string GenerateExpectedResult(string stepDescription, XElement testingProcedure = null)
        //{
        //    // Ưu tiên lấy kết quả kỳ vọng từ XML nếu có
        //    string expectedResult = testingProcedure?.Attribute("ExpectedResult")?.Value;
        //    if (!string.IsNullOrEmpty(expectedResult))
        //    {
        //        return expectedResult;
        //    }

        //    // Chuyển mô tả bước về chữ thường để xử lý dễ dàng
        //    stepDescription = stepDescription.ToLower();

        //    // Tách các từ trong mô tả bước
        //    var words = stepDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //    // Xác định chủ ngữ (từ đầu tiên hoặc hai từ đầu tiên)
        //    string subject = words.Length > 0 ? words[0] : "";
        //    if (subject == "khách" && words.Length > 1 && words[1] == "hàng")
        //    {
        //        subject = "khách hàng";
        //    }
        //    else if (subject == "hệ" && words.Length > 1 && words[1] == "thống")
        //    {
        //        subject = "hệ thống";
        //    }

        //    // Xác định hành động chính (động từ chính)
        //    string action = "";
        //    string actionObject = ""; // Đối tượng của hành động (nếu có)
        //    for (int i = 0; i < words.Length; i++)
        //    {
        //        if (words[i] == "nhập" || words[i] == "chọn" || words[i] == "yêu" || words[i] == "kiểm" || words[i] == "xác" || words[i] == "lưu" || words[i] == "trả" || words[i] == "liên" || words[i] == "tạo")
        //        {
        //            action = words[i];
        //            // Lấy đối tượng của hành động (các từ sau động từ)
        //            if (i < words.Length - 1)
        //            {
        //                actionObject = string.Join(" ", words.Skip(i + 1));
        //            }
        //            break;
        //        }
        //    }

        //    // Suy luận kết quả kỳ vọng dựa trên chủ ngữ và hành động
        //    if (subject == "khách hàng")
        //    {
        //        if (action == "nhập" || action == "chọn")
        //        {
        //            if (string.IsNullOrEmpty(actionObject))
        //            {
        //                return "Hệ thống ghi nhận thông tin khách hàng nhập.";
        //            }
        //            return $"Hệ thống ghi nhận {actionObject} mà khách hàng đã nhập.";
        //        }
        //    }
        //    else if (subject == "hệ thống")
        //    {
        //        if (action == "yêu")
        //        {
        //            if (string.IsNullOrEmpty(actionObject))
        //            {
        //                return "Hệ thống hiển thị thông báo yêu cầu nhập thông tin, bao gồm các trường bắt buộc (có dấu *).";
        //            }
        //            return $"Hệ thống hiển thị thông báo khi khách hàng nhập thiếu thông tin ở các trường bắt buộc (có dấu *) trong {actionObject}.";
        //        }
        //        else if (action == "kiểm")
        //        {
        //            if (stepDescription.Contains("hệ thống kiểm tra"))
        //            {
        //                // Xử lý bước kiểm tra trong luồng phụ
        //                if (stepDescription.Contains("dữ liệu cơ bản của khách hàng không đầy đủ"))
        //                {
        //                    return "Hệ thống hiển thị thông báo: 'Dữ liệu cơ bản của khách hàng không đầy đủ, vui lòng nhập lại.'";
        //                }
        //                else if (stepDescription.Contains("địa chỉ không hợp lệ"))
        //                {
        //                    return "Hệ thống hiển thị thông báo: 'Địa chỉ không hợp lệ, vui lòng nhập lại.'";
        //                }
        //                else if (stepDescription.Contains("thông tin thẻ tín dụng/ghi nợ không hợp lệ"))
        //                {
        //                    return "Hệ thống hiển thị thông báo: 'Thông tin thẻ tín dụng/ghi nợ không hợp lệ, vui lòng nhập lại.'";
        //                }
        //            }
        //            if (string.IsNullOrEmpty(actionObject))
        //            {
        //                return "Hệ thống xác nhận thông tin hợp lệ.";
        //            }
        //            return $"Hệ thống xác nhận {actionObject} hợp lệ.";
        //        }
        //        else if (action == "xác")
        //        {
        //            if (string.IsNullOrEmpty(actionObject))
        //            {
        //                return "Hệ thống xác nhận thông tin hợp lệ.";
        //            }
        //            return $"Hệ thống xác nhận {actionObject} hợp lệ.";
        //        }
        //        else if (action == "lưu" || action == "trả")
        //        {
        //            return "Hệ thống xác nhận thông tin hợp lệ và lưu trữ dữ liệu thành công.";
        //        }
        //        else if (action == "liên")
        //        {
        //            return $"Hệ thống liên kết {actionObject} thành công, đảm bảo dữ liệu được lưu trữ chính xác.";
        //        }
        //        // Phần đã sửa: Cải thiện kết quả kỳ vọng cho các bước "Hệ thống tạo"
        //        else if (action == "tạo")
        //        {
        //            if (string.IsNullOrEmpty(actionObject))
        //            {
        //                return "Hệ thống tạo bản ghi thành công.";
        //            }
        //            if (actionObject.Contains("bản ghi khách hàng"))
        //            {
        //                return "Hệ thống tạo bản ghi khách hàng mới thành công, bản ghi được lưu trong cơ sở dữ liệu với các trường thông tin cơ bản.";
        //            }
        //            else if (actionObject.Contains("bản ghi địa chỉ"))
        //            {
        //                return "Hệ thống tạo bản ghi địa chỉ thành công, địa chỉ được lưu trong cơ sở dữ liệu với định dạng hợp lệ.";
        //            }
        //            else if (actionObject.Contains("tài khoản khách hàng"))
        //            {
        //                return "Hệ thống tạo tài khoản khách hàng thành công, tài khoản được lưu trong cơ sở dữ liệu với thông tin thẻ tín dụng/ghi nợ.";
        //            }
        //            return $"Hệ thống tạo {actionObject} thành công.";
        //        }
        //    }

        //    // Nếu không suy luận được, trả về kết quả cụ thể hơn dựa trên ngữ cảnh
        //    if (stepDescription.Contains("khách hàng"))
        //    {
        //        return "Hệ thống ghi nhận hành động của khách hàng và chuyển sang bước tiếp theo.";
        //    }
        //    else if (stepDescription.Contains("hệ thống"))
        //    {
        //        return "Hệ thống thực hiện hành động thành công và chuyển sang bước tiếp theo.";
        //    }

        //    return "Hệ thống phản hồi phù hợp với hành động được thực hiện.";
        //}


        private string GenerateExpectedResult(string stepDescription, XElement testingProcedure = null)
        {
            // Ưu tiên lấy kết quả kỳ vọng từ XML nếu có (trong trường hợp này không áp dụng, nhưng giữ lại để tương thích)
            string expectedResult = testingProcedure?.Attribute("ExpectedResult")?.Value;
            if (!string.IsNullOrEmpty(expectedResult))
            {
                return expectedResult;
            }

            // Chuyển mô tả bước về chữ thường để xử lý dễ dàng
            stepDescription = stepDescription.ToLower();

            // Tách các từ trong mô tả bước
            var words = stepDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Xác định chủ ngữ (từ đầu tiên hoặc hai từ đầu tiên)
            string subject = words.Length > 0 ? words[0] : "";
            if (subject == "khách" && words.Length > 1 && words[1] == "hàng")
            {
                subject = "khách hàng";
            }
            else if (subject == "hệ" && words.Length > 1 && words[1] == "thống")
            {
                subject = "hệ thống";
            }

            // Xác định hành động chính (động từ chính)
            string action = "";
            string actionObject = ""; // Đối tượng của hành động (nếu có)
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "nhập" || words[i] == "chọn" || words[i] == "yêu" || words[i] == "kiểm" || words[i] == "xác" || words[i] == "lưu" || words[i] == "trả" || words[i] == "liên" || words[i] == "tạo")
                {
                    action = words[i];
                    // Lấy đối tượng của hành động (các từ sau động từ)
                    if (i < words.Length - 1)
                    {
                        actionObject = string.Join(" ", words.Skip(i + 1));
                    }
                    break;
                }
            }

            // Suy luận kết quả kỳ vọng dựa trên chủ ngữ và hành động
            if (subject == "khách hàng")
            {
                if (action == "nhập")
                {
                    if (string.IsNullOrEmpty(actionObject))
                    {
                        return "Hệ thống ghi nhận thông tin khách hàng nhập và chuyển sang bước tiếp theo.";
                    }
                    if (actionObject.Contains("thông tin cơ bản"))
                    {
                        return "Hệ thống ghi nhận thông tin cơ bản của khách hàng (họ tên, ngày sinh, số điện thoại, email) và chuyển sang bước tiếp theo.";
                    }
                    else if (actionObject.Contains("địa chỉ"))
                    {
                        return "Hệ thống ghi nhận địa chỉ của khách hàng và chuyển sang bước tiếp theo.";
                    }
                    else if (actionObject.Contains("thông tin thẻ tín dụng"))
                    {
                        return "Hệ thống ghi nhận thông tin thẻ tín dụng/ghi nợ của khách hàng và chuyển sang bước kiểm tra thông tin.";
                    }
                    return $"Hệ thống ghi nhận {actionObject} mà khách hàng đã nhập và chuyển sang bước tiếp theo.";
                }
                else if (action == "chọn")
                {
                    if (string.IsNullOrEmpty(actionObject))
                    {
                        return "Hệ thống ghi nhận lựa chọn của khách hàng và chuyển sang bước tiếp theo.";
                    }
                    return $"Hệ thống ghi nhận {actionObject} mà khách hàng đã chọn và chuyển sang bước tiếp theo.";
                }
            }
            else if (subject == "hệ thống")
            {
                if (action == "yêu")
                {
                    if (string.IsNullOrEmpty(actionObject))
                    {
                        return "Hệ thống hiển thị thông báo yêu cầu nhập thông tin, bao gồm các trường bắt buộc (có dấu *).";
                    }
                    return $"Hệ thống hiển thị thông báo yêu cầu nhập {actionObject}, bao gồm các trường bắt buộc (có dấu *).";
                }
                else if (action == "kiểm" || action == "xác")
                {
                    if (stepDescription.Contains("hệ thống kiểm tra") || stepDescription.Contains("hệ thống xác minh"))
                    {
                        // Xử lý các bước kiểm tra trong luồng chính
                        if (stepDescription.Contains("thông tin cơ bản"))
                        {
                            return "Hệ thống xác nhận thông tin cơ bản của khách hàng (họ tên, ngày sinh, số điện thoại, email) hợp lệ.";
                        }
                        else if (stepDescription.Contains("địa chỉ"))
                        {
                            return "Hệ thống xác nhận địa chỉ của khách hàng hợp lệ.";
                        }
                        else if (stepDescription.Contains("thông tin thẻ tín dụng"))
                        {
                            return "Hệ thống xác nhận thông tin thẻ tín dụng/ghi nợ hợp lệ.";
                        }
                    }
                    if (string.IsNullOrEmpty(actionObject))
                    {
                        return "Hệ thống xác nhận thông tin hợp lệ.";
                    }
                    return $"Hệ thống xác nhận {actionObject} hợp lệ.";
                }
                else if (action == "lưu" || action == "trả")
                {
                    if (stepDescription.Contains("khách hàng"))
                    {
                        return "Hệ thống lưu trữ thông tin khách hàng thành công vào cơ sở dữ liệu.";
                    }
                    return "Hệ thống xác nhận thông tin hợp lệ và lưu trữ dữ liệu thành công.";
                }
                else if (action == "liên")
                {
                    return $"Hệ thống liên kết {actionObject} thành công, đảm bảo dữ liệu được lưu trữ chính xác.";
                }
                else if (action == "tạo")
                {
                    if (string.IsNullOrEmpty(actionObject))
                    {
                        return "Hệ thống tạo bản ghi thành công.";
                    }
                    if (actionObject.Contains("bản ghi khách hàng"))
                    {
                        return "Hệ thống tạo bản ghi khách hàng mới thành công, bao gồm các trường thông tin cơ bản (họ tên, ngày sinh, số điện thoại, email), và lưu vào cơ sở dữ liệu.";
                    }
                    else if (actionObject.Contains("bản ghi địa chỉ"))
                    {
                        return "Hệ thống tạo bản ghi địa chỉ thành công, địa chỉ được lưu vào cơ sở dữ liệu với định dạng hợp lệ.";
                    }
                    else if (actionObject.Contains("tài khoản khách hàng"))
                    {
                        return "Hệ thống tạo tài khoản khách hàng thành công, bao gồm thông tin thẻ tín dụng/ghi nợ, và lưu vào cơ sở dữ liệu.";
                    }
                    return $"Hệ thống tạo {actionObject} thành công.";
                }
            }

            // Nếu không suy luận được, trả về kết quả mặc định
            if (stepDescription.Contains("khách hàng"))
            {
                return "Hệ thống ghi nhận hành động của khách hàng và chuyển sang bước tiếp theo.";
            }
            else if (stepDescription.Contains("hệ thống"))
            {
                return "Hệ thống thực hiện hành động thành công và chuyển sang bước tiếp theo.";
            }

            return "Hệ thống phản hồi phù hợp với hành động được thực hiện.";
        }


        // Sinh thông báo lỗi trong thẻ Extensions
        private string GenerateErrorMessage(string condition)
        {
            condition = condition.ToLower();
            if (condition.Contains("không hợp lệ"))
            {
                return $"Hệ thống hiển thị thông báo: '{condition}, vui lòng nhập lại.'";
            }
            else if (condition.Contains("không đầy đủ"))
            {
                return $"Hệ thống hiển thị thông báo: '{condition}, vui lòng nhập lại.'";
            }

            return $"Hệ thống hiển thị thông báo: '{condition}'.";
        }

        // Hàm sinh test case từ file XML và lưu vào danh sách _testCases
        private void GenerateTestCases()
        {

            try
            {
                _testCases.Clear();
                txtThongbao.Clear();

                // Tìm UseCase theo Id
                var useCase = xmlDoc.Descendants("UseCase")
                    .FirstOrDefault(uc => uc.Attribute("Id")?.Value == selectedUseCaseId);

                if (useCase == null)
                {
                    txtThongbao.AppendText("Không tìm thấy Use Case!\r\n");
                    return;
                }

                string useCaseName = useCase.Attribute("Name")?.Value ?? "Unknown Use Case";
                string useCaseId = useCase.Attribute("Id")?.Value ?? "UC-Unknown"; // Đảm bảo có Id

                // Lấy postconditions (nếu có), nếu không thì dùng mặc định
                var taggedValues = useCase.Element("TaggedValues")?.Element("TaggedValueContainer")?.Elements("TaggedValue");
                var postconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Post-conditions")?.Attribute("Value")?.Value
                    ?? "Hệ thống xác nhận thông tin hợp lệ và lưu.";

                // Tìm StepContainer hoặc các thẻ tương tự (MainFlow, Flow, ...)
                var stepContainer = useCase.Element("StepContainers")?.Element("StepContainer")
                    ?? useCase.Element("MainFlow") // Hỗ trợ thẻ MainFlow nếu có
                    ?? useCase.Element("Flow"); // Hỗ trợ thẻ Flow nếu có

                if (stepContainer == null)
                {
                    txtThongbao.AppendText("Không tìm thấy StepContainer hoặc Flow trong Use Case! Vui lòng kiểm tra cấu trúc XML.\r\n");
                    return;
                }

                // Tìm Steps, hỗ trợ các tên thẻ khác nhau
                var steps = stepContainer.Element("Steps")?.Elements("Step")
                    ?? stepContainer.Elements("Step") // Hỗ trợ nếu Steps không tồn tại
                    ?? stepContainer.Elements("Action"); // Hỗ trợ thẻ Action nếu có

                if (steps == null || !steps.Any())
                {
                    txtThongbao.AppendText("Không tìm thấy bước nào trong Use Case! Vui lòng kiểm tra cấu trúc XML.\r\n");
                    return;
                }

                // Lưu danh sách tất cả các bước và sub-steps để dùng cho luồng chính và luồng phụ
                var allStepsWithSubSteps = new List<(XElement Step, int StepIndex, List<(XElement SubStep, int SubStepIndex)> SubSteps)>();
                int stepIndex = 1;

                foreach (var step in steps)
                {
                    var subStepsList = new List<(XElement SubStep, int SubStepIndex)>();
                    var subSteps = step.Element("Steps")?.Elements("Step")
                        ?? step.Elements("Step") // Hỗ trợ nếu không có thẻ Steps
                        ?? step.Elements("SubStep") // Hỗ trợ thẻ SubStep nếu có
                        ?? Enumerable.Empty<XElement>();

                    int subStepIndex = 1;
                    foreach (var subStep in subSteps)
                    {
                        subStepsList.Add((subStep, subStepIndex));
                        subStepIndex++;
                    }

                    allStepsWithSubSteps.Add((step, stepIndex, subStepsList));
                    stepIndex++;
                }

                // Test Case 1: Main Success Scenario (TC-01)
                var mainSteps = new List<string>();
                var mainExpectedResults = new List<string>();
                int currentStepIndex = 1;

                foreach (var (step, stepIdx, subSteps) in allStepsWithSubSteps)
                {
                    string stepName = step.Attribute("Name")?.Value
                        ?? step.Attribute("Description")?.Value // Hỗ trợ thuộc tính Description
                        ?? $"Step {stepIdx}";

                    var testingProcedure = step.Element("TestingProcedures")?.Elements("TestingProcedure").FirstOrDefault()
                        ?? step.Element("ExpectedResults")?.Elements("Result").FirstOrDefault(); // Hỗ trợ thẻ ExpectedResults

                    // Thêm bước chính
                    mainSteps.Add($"{currentStepIndex}. {stepName}");
                    mainExpectedResults.Add(GenerateExpectedResult(stepName, testingProcedure));
                    currentStepIndex++;

                    // Thêm sub-steps (nếu có)
                    foreach (var (subStep, subStepIdx) in subSteps)
                    {
                        string subStepName = subStep.Attribute("Name")?.Value
                            ?? subStep.Attribute("Description")?.Value // Hỗ trợ thuộc tính Description
                            ?? $"Sub-step {subStepIdx}";

                        testingProcedure = subStep.Element("TestingProcedures")?.Elements("TestingProcedure").FirstOrDefault()
                            ?? subStep.Element("ExpectedResults")?.Elements("Result").FirstOrDefault();

                        // Bỏ qua các bước không cần thiết (như "Hệ thống tạo bản ghi")
                        if (subStepName.ToLower().Contains("hệ thống tạo bản ghi"))
                        {
                            continue;
                        }

                        mainSteps.Add($"{currentStepIndex}. {subStepName}");
                        mainExpectedResults.Add(GenerateExpectedResult(subStepName, testingProcedure));
                        currentStepIndex++;
                    }
                }

                // Thêm bước kiểm tra và lưu nếu không có bước nào liên quan đến kiểm tra/xác minh hoặc lưu
                bool hasValidationOrSaveStep = mainSteps.Any(s => s.ToLower().Contains("hệ thống kiểm tra") ||
                                                                 s.ToLower().Contains("hệ thống xác minh") ||
                                                                 s.ToLower().Contains("hệ thống xác nhận") ||
                                                                 s.ToLower().Contains("hệ thống lưu"));
                bool hasValidationOrSaveResult = mainExpectedResults.Any(r => r.ToLower().Contains("hệ thống xác nhận") ||
                                                                             r.ToLower().Contains("hệ thống lưu"));
                if (!hasValidationOrSaveStep && !hasValidationOrSaveResult)
                {
                    mainSteps.Add($"{currentStepIndex}. Hệ thống kiểm tra và lưu thông tin.");
                    mainExpectedResults.Add(postconditions);
                }

                var mainTestCase = new TestCase
                {
                    UseCase = useCaseId, // Sử dụng useCaseId từ XML
                    TestName = "TC-01",
                    UseCaseName = useCaseName,
                    Procedure = string.Join("\n", mainSteps),
                    ExpectedResults = string.Join("\n", mainExpectedResults)
                };
                _testCases.Add(mainTestCase);

                // Test Cases: Alternative Flows (Extensions)
                int testCaseCounter = 2;

                for (int i = 0; i < allStepsWithSubSteps.Count; i++)
                {
                    var (step, stepIdx, subSteps) = allStepsWithSubSteps[i];

                    for (int j = 0; j < subSteps.Count; j++)
                    {
                        var (subStep, subStepIdx) = subSteps[j];
                        var extensions = subStep.Element("Extensions")?.Elements("Extension")
                            ?? subStep.Element("Alternatives")?.Elements("Alternative") // Hỗ trợ thẻ Alternatives
                            ?? Enumerable.Empty<XElement>();

                        foreach (var extension in extensions)
                        {
                            string condition = extension.Attribute("Name")?.Value
                                ?? extension.Attribute("Condition")?.Value; // Hỗ trợ thuộc tính Condition

                            if (string.IsNullOrEmpty(condition))
                            {
                                txtThongbao.AppendText($"Cảnh báo: Extension không có thuộc tính Name hoặc Condition trong sub-step '{subStep.Attribute("Name")?.Value}'.\r\n");
                                continue;
                            }

                            var altSteps = new List<string>();
                            var altExpectedResults = new List<string>();
                            int altStepIndex = 1;

                            // Thêm các bước từ đầu đến bước chính hiện tại
                            for (int k = 0; k <= i; k++)
                            {
                                var (currentStep, currentStepIdx, currentSubSteps) = allStepsWithSubSteps[k];
                                string currentStepName = currentStep.Attribute("Name")?.Value
                                    ?? currentStep.Attribute("Description")?.Value
                                    ?? $"Step {currentStepIdx}";

                                var stepTestingProc = currentStep.Element("TestingProcedures")?.Elements("TestingProcedure").FirstOrDefault()
                                    ?? currentStep.Element("ExpectedResults")?.Elements("Result").FirstOrDefault();

                                altSteps.Add($"{altStepIndex}. {currentStepName}");
                                altExpectedResults.Add(GenerateExpectedResult(currentStepName, stepTestingProc));
                                altStepIndex++;

                                // Thêm sub-steps (nếu có) của bước chính hiện tại
                                int subStepsToInclude = (k == i) ? j + 1 : currentSubSteps.Count;
                                for (int m = 0; m < subStepsToInclude; m++)
                                {
                                    var (currentSubStep, currentSubStepIdx) = currentSubSteps[m];
                                    string currentSubStepName = currentSubStep.Attribute("Name")?.Value
                                        ?? currentSubStep.Attribute("Description")?.Value
                                        ?? $"Sub-step {currentSubStepIdx}";

                                    var subStepTestingProc = currentSubStep.Element("TestingProcedures")?.Elements("TestingProcedure").FirstOrDefault()
                                        ?? currentSubStep.Element("ExpectedResults")?.Elements("Result").FirstOrDefault();

                                    if (currentSubStepName.ToLower().Contains("hệ thống tạo bản ghi"))
                                    {
                                        continue;
                                    }

                                    altSteps.Add($"{altStepIndex}. {currentSubStepName}");
                                    altExpectedResults.Add(GenerateExpectedResult(currentSubStepName, subStepTestingProc));
                                    altStepIndex++;
                                }
                            }

                            // Thêm bước kiểm tra với điều kiện extension
                            altSteps.Add($"{altStepIndex}. Hệ thống kiểm tra ({condition}).");
                            altExpectedResults.Add(GenerateErrorMessage(condition));

                            var altTestCase = new TestCase
                            {
                                UseCase = useCaseId, // Sử dụng useCaseId từ XML
                                TestName = $"TC-{testCaseCounter:D2}",
                                UseCaseName = useCaseName,
                                Procedure = string.Join("\n", altSteps),
                                ExpectedResults = string.Join("\n", altExpectedResults)
                            };
                            _testCases.Add(altTestCase);
                            testCaseCounter++;
                        }
                    }
                }

                if (_testCases.Count == 0)
                {
                    txtThongbao.AppendText("Không sinh được test case nào từ Use Case!\r\n");
                }
                else
                {
                    txtThongbao.AppendText($"Đã sinh ra {_testCases.Count} test case từ Use Case!\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi sinh test case: {ex.Message}\r\n");
            }
        }

        //sự kiện cho nút Sinh test case
        private void btnGenerate_Click(object sender, EventArgs e)
        {

            //// Kiểm tra xem đã chọn file và thư mục chưa            
            //if (string.IsNullOrEmpty(txtInputTM.Text))
            //{
            //    txtThongbao.AppendText("Vui lòng chọn file đặc tả!\r\n");
            //    return;
            //}

            //if (string.IsNullOrEmpty(txtOutputTM.Text))
            //{
            //    txtThongbao.AppendText("Vui lòng chọn thư mục đầu ra!\r\n");
            //    return;
            //}

            //try
            //{
            //    txtThongbao.Clear(); // Xóa thông báo cũ

            //    // Nếu xử lý file XML
            //    if (radioXMLIn.Checked)
            //    {
            //        // Kiểm tra xem người dùng đã chọn Use Case chưa
            //        if (comboboxUC.SelectedIndex == -1 || string.IsNullOrEmpty(selectedUseCaseId))
            //        {
            //            txtThongbao.AppendText("Chưa chọn Use Case!\r\n");
            //            return;
            //        }

            //        // Dùng phương thức cụ thể cho XML hiện tại
            //        GenerateTestCases();
            //    }
            //    // Nếu xử lý file .txt
            //    else if (radioTextIn.Checked)
            //    {
            //        if (string.IsNullOrEmpty(_selectedFilePath))
            //        {
            //            txtThongbao.AppendText("Không có file .txt nào được chọn!\r\n");
            //            return;
            //        }
            //        ParseTxtFile(_selectedFilePath);
            //    }
            //    else if (radioXMIIn.Checked)
            //    {
            //        txtThongbao.AppendText("Chức năng xử lý file XMI chưa được triển khai.\r\n");
            //        return;
            //    }

            //    if (_testCases == null || _testCases.Count == 0)
            //    {
            //        txtThongbao.AppendText("Không có test case nào để sinh file đầu ra. Vui lòng kiểm tra file và thử lại.\r\n");
            //        return;
            //    }

            //    string outputFolder = txtOutputTM.Text;
            //    string outputFormat = radioWordOut.Checked ? "Word" : radioExcelOut.Checked ? "Excel" : "HTML";
            //    string fileExtension = outputFormat.ToLower() == "excel" ? "xlsx" :
            //                          outputFormat.ToLower() == "word" ? "txt" :
            //                          "html";
            //    string outputFile = Path.Combine(outputFolder, $"TestCases_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}");

            //    // Sinh file đầu ra
            //    GenerateOutputFile(_testCases, outputFile, outputFormat);
            //    txtThongbao.AppendText($"Đã sinh file đầu ra tại: {outputFile}\r\n");
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi sinh file đầu ra: {ex.Message}\r\n");
            //}

            // Kiểm tra xem đã chọn file và thư mục chưa            
            if (string.IsNullOrEmpty(txtInputTM.Text))
            {
                txtThongbao.AppendText("Vui lòng chọn file đặc tả!\r\n");
                return;
            }

            if (string.IsNullOrEmpty(txtOutputTM.Text))
            {
                txtThongbao.AppendText("Vui lòng chọn thư mục đầu ra!\r\n");
                return;
            }

            if (!radioWordOut.Checked && !radioExcelOut.Checked && !radioHTMLOut.Checked)
            {
                txtThongbao.AppendText("Vui lòng chọn định dạng đầu ra!\r\n");
                return;
            }

            try
            {
                txtThongbao.Clear(); // Xóa thông báo cũ

                // Nếu xử lý file XML
                if (radioXMLIn.Checked)
                {
                    // Kiểm tra xem người dùng đã chọn Use Case chưa
                    if (comboboxUC.SelectedIndex == -1 || string.IsNullOrEmpty(selectedUseCaseId))
                    {
                        txtThongbao.AppendText("Chưa chọn Use Case!\r\n");
                        return;
                    }

                    // Dùng phương thức cụ thể cho XML hiện tại
                    GenerateTestCases();
                }
                // Nếu xử lý file .txt
                else if (radioTextIn.Checked)
                {
                    if (string.IsNullOrEmpty(_selectedFilePath))
                    {
                        txtThongbao.AppendText("Không có file .txt nào được chọn!\r\n");
                        return;
                    }
                    ParseTxtFile(_selectedFilePath);
                }
                else if (radioXMIIn.Checked)
                {
                    // Kiểm tra xem người dùng đã chọn Use Case chưa
                    if (comboboxUC.SelectedIndex == -1 || string.IsNullOrEmpty(selectedUseCaseId))
                    {
                        txtThongbao.AppendText("Chưa chọn Use Case!\r\n");
                        return;
                    }

                    // Dùng phương thức cụ thể cho XMI
                    GenerateTestCasesFromXmi();
                }

                if (_testCases == null || _testCases.Count == 0)
                {
                    txtThongbao.AppendText("Không có test case nào để sinh file đầu ra. Vui lòng kiểm tra file và thử lại.\r\n");
                    return;
                }

                string outputFolder = txtOutputTM.Text;
                string outputFormat = radioWordOut.Checked ? "Word" : radioExcelOut.Checked ? "Excel" : "HTML";
                string fileExtension = outputFormat.ToLower() == "excel" ? "xlsx" :
                                      outputFormat.ToLower() == "word" ? "txt" :
                                      "html";
                string outputFile = Path.Combine(outputFolder, $"TestCases_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}");

                // Sinh file đầu ra
                GenerateOutputFile(_testCases, outputFile, outputFormat);
                txtThongbao.AppendText($"Đã sinh file đầu ra tại: {outputFile}\r\n");
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi sinh file đầu ra: {ex.Message}\r\n");
            }

        }

        private void GenerateOutputFile(List<TestCase> testCases, string filePath, string format)
        {
            if (format == "Excel")
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "TestCases" };
                    sheets.Append(sheet);

                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // Tiêu đề
                    Row headerRow = new Row();
                    headerRow.Append(
                        new Cell() { CellValue = new CellValue("Use Case"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Use Case Name"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Test Case"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Procedure"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Expected Result"), DataType = CellValues.String }
                    );
                    sheetData.Append(headerRow);

                    // Dữ liệu
                    foreach (var tc in testCases)
                    {
                        Row row = new Row();
                        row.Append(
                            new Cell() { CellValue = new CellValue(tc.UseCase), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.UseCaseName), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.TestName), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.Procedure), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.ExpectedResults), DataType = CellValues.String }
                        );
                        sheetData.Append(row);
                    }

                    workbookPart.Workbook.Save();
                }
            }
            else if (format == "HTML")
            {
                StringBuilder html = new StringBuilder();
                html.AppendLine("<html><body><h1>Test Cases</h1>");
                html.AppendLine("<table border='1'><tr>");
                html.AppendLine("<th>Use Case</th><th>Use Case Name</th><th>Test Case</th><th>Procedure</th><th>Expected Result</th></tr>");

                foreach (var tc in testCases)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{tc.UseCase}</td>");
                    html.AppendLine($"<td>{tc.UseCaseName}</td>");
                    html.AppendLine($"<td>{tc.TestName}</td>");
                    html.AppendLine($"<td>{tc.Procedure.Replace("\n", "<br>")}</td>");
                    html.AppendLine($"<td>{tc.ExpectedResults.Replace("\n", "<br>")}</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</table></body></html>");
                File.WriteAllText(filePath, html.ToString());
            }
            else // Word (dùng định dạng đơn giản dạng text)
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine("Test Cases");
                text.AppendLine(new string('=', 50));
                foreach (var tc in testCases)
                {
                    text.AppendLine($"Use Case: {tc.UseCase}");
                    text.AppendLine($"Use Case Name: {tc.UseCaseName}");
                    text.AppendLine($"Test Case: {tc.TestName}");
                    text.AppendLine($"Procedure: {tc.Procedure}");
                    text.AppendLine($"Expected Result: {tc.ExpectedResults}");
                    text.AppendLine(new string('-', 50));
                }
                File.WriteAllText(filePath, text.ToString());
            }
        }



        //sự kiện cho nút làm mới
        private void btnReset_Click(object sender, EventArgs e)
        {
            
            txtInputTM.Clear();
            txtOutputTM.Clear();
            txtThongbao.Clear();
            _testCases.Clear();
            _selectedFilePath = null;
            selectedUseCaseId = null;
            comboboxUC.Items.Clear(); // Xóa danh sách Use Case
            comboboxUC.SelectedIndex = -1; // Đặt lại ComboBox
            radioTextIn.Checked = false;
            radioXMLIn.Checked = true; // Mặc định chọn XML
            radioXMIIn.Checked = false;
            radioWordOut.Checked = true; // Mặc định chọn Word
            radioExcelOut.Checked = false;
            radioHTMLOut.Checked = false;
            txtThongbao.AppendText("Đã làm mới các lựa chọn.\r\n");
        }



        //Hàm đọc file XMI
        private bool ValidateXmiFile(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    XDocument.Parse(reader.ReadToEnd()); // Thử phân tích cú pháp XMI
                }
                txtThongbao.AppendText("File XMI hợp lệ về mặt cú pháp.\r\n");
                return true;
            }
            catch (System.Xml.XmlException xmlEx)
            {
                txtThongbao.AppendText($"File XMI không hợp lệ về mặt cú pháp:\r\n");
                txtThongbao.AppendText($"Lỗi: {xmlEx.Message}\r\n");
                txtThongbao.AppendText($"Dòng: {xmlEx.LineNumber}, Vị trí: {xmlEx.LinePosition}\r\n");
                txtThongbao.AppendText("Vui lòng kiểm tra lại cấu trúc file XMI.\r\n");
                return false;
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi không xác định khi kiểm tra file XMI:\r\n");
                txtThongbao.AppendText($"Lỗi: {ex.Message}\r\n");
                txtThongbao.AppendText($"Stack Trace: {ex.StackTrace}\r\n");
                txtThongbao.AppendText("Vui lòng kiểm tra file hoặc liên hệ hỗ trợ.\r\n");
                return false;
            }
        }


        public class UseCaseStep
        {
            public string Name { get; set; }
            public List<UseCaseStep> SubSteps { get; set; } = new List<UseCaseStep>();
            public List<UseCaseStep> Extensions { get; set; } = new List<UseCaseStep>();
        }
        private UseCaseStep ParseXmiStep(XElement stepElement)
        {
            var step = new UseCaseStep
            {
                Name = stepElement.Element("properties")?.Elements("property")
                    .FirstOrDefault(p => p.Attribute("name")?.Value == "name")?.Attribute("value")?.Value ?? "Unnamed Step"
            };

            var subStepsContainer = stepElement.Descendants("stepContainers").FirstOrDefault();
            if (subStepsContainer != null)
            {
                foreach (var subStep in subStepsContainer.Elements("vpumlModel"))
                {
                    var subStepObj = ParseXmiStep(subStep);
                    // Phân loại bước: nếu là bước "không hợp lệ" hoặc "không đầy đủ", thì là Extension
                    if (subStepObj.Name.ToLower().Contains("không hợp lệ") || subStepObj.Name.ToLower().Contains("không đầy đủ"))
                    {
                        step.Extensions.Add(subStepObj);
                    }
                    else
                    {
                        step.SubSteps.Add(subStepObj);
                    }
                }
            }

            return step;
        }

        

        private void GenerateTestCasesFromXmi()
        {
            try
            {
                _testCases.Clear();
                txtThongbao.Clear();

                txtThongbao.AppendText($"Đang tìm Use Case với ID: {selectedUseCaseId}\r\n");

                // Tìm UseCase theo Id trong file XMI
                var useCase = xmlDoc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}packagedElement")
                    .Where(uc => uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}type")?.Value == "uml:UseCase")
                    .FirstOrDefault(uc => uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value == selectedUseCaseId);

                if (useCase == null)
                {
                    txtThongbao.AppendText("Không tìm thấy Use Case!\r\n");
                    return;
                }

                string useCaseId = selectedUseCaseId;
                string useCaseName = useCase.Attribute("name")?.Value ?? "Unknown Use Case";

                // Lấy preconditions và postconditions từ TC1_profile:UseCase
                var useCaseProfile = xmlDoc.Descendants("{http:///schemas/TC1_profile/0}UseCase")
                    .FirstOrDefault(uc => uc.Attribute("base_UseCase")?.Value == selectedUseCaseId);
                var preconditions = useCaseProfile?.Attribute("Preconditions")?.Value?.Replace("\n", "\n") ?? "Không có điều kiện trước.";
                var postconditions = useCaseProfile?.Attribute("Post-conditions")?.Value?.Replace("\n", "\n") ?? "Hệ thống xác nhận thông tin hợp lệ và lưu.";

                // Tìm tất cả TestingProcedure liên quan đến Use Case này
                var testingProcedures = xmlDoc.Descendants("vpumlModel")
                    .Where(tp => tp.Attribute("modelType")?.Value == "TestingProcedure")
                    .Where(tp => tp.Descendants("vpumlModel")
                        .Any(step => step.Element("properties")?.Elements("property")
                            .Any(prop => prop.Attribute("name")?.Value == "name" && prop.Attribute("value")?.Value == useCaseName) ?? false))
                    .ToList();

                if (!testingProcedures.Any())
                {
                    txtThongbao.AppendText($"Không tìm thấy Testing Procedure cho Use Case '{useCaseName}'!\r\n");
                    return;
                }

                int testCaseCounter = 1;
                int procedureIndex = 1;

                // Lặp qua từng TestingProcedure
                foreach (var tp in testingProcedures)
                {
                    txtThongbao.AppendText($"Đang xử lý Testing Procedure {procedureIndex} cho Use Case '{useCaseName}'...\r\n");

                    // Lấy tất cả các bước từ TestingProcedure
                    var flowOfEvents = new List<UseCaseStep>();
                    var stepContainers = tp.Descendants("stepContainers");
                    foreach (var container in stepContainers)
                    {
                        foreach (var step in container.Elements("vpumlModel"))
                        {
                            var stepObj = ParseXmiStep(step);
                            flowOfEvents.Add(stepObj);
                        }
                    }

                    if (!flowOfEvents.Any())
                    {
                        txtThongbao.AppendText($"Không tìm thấy bước nào trong Testing Procedure {procedureIndex} cho Use Case '{useCaseName}'!\r\n");
                        continue;
                    }

                    // Sinh Test Case cho Main Flow của Testing Procedure này
                    var mainSteps = new List<string>();
                    var mainExpectedResults = new List<string>();
                    int currentStepIndex = 1;

                    foreach (var step in flowOfEvents)
                    {
                        mainSteps.Add($"{currentStepIndex}. {step.Name}");
                        mainExpectedResults.Add(GenerateExpectedResult(step.Name));
                        currentStepIndex++;

                        // Thêm sub-steps (nếu có)
                        foreach (var subStep in step.SubSteps)
                        {
                            mainSteps.Add($"{currentStepIndex}. {subStep.Name}");
                            mainExpectedResults.Add(GenerateExpectedResult(subStep.Name));
                            currentStepIndex++;
                        }
                    }

                    // Thêm bước kiểm tra và lưu nếu cần
                    bool hasValidationOrSaveStep = mainSteps.Any(s => s.ToLower().Contains("hệ thống kiểm tra") ||
                                                                     s.ToLower().Contains("hệ thống xác minh") ||
                                                                     s.ToLower().Contains("hệ thống xác nhận") ||
                                                                     s.ToLower().Contains("hệ thống lưu"));
                    bool hasValidationOrSaveResult = mainExpectedResults.Any(r => r.ToLower().Contains("hệ thống xác nhận") ||
                                                                                 r.ToLower().Contains("hệ thống lưu"));
                    if (!hasValidationOrSaveStep && !hasValidationOrSaveResult)
                    {
                        mainSteps.Add($"{currentStepIndex}. Hệ thống kiểm tra và lưu thông tin.");
                        mainExpectedResults.Add(postconditions);
                    }

                    var mainTestCase = new TestCase
                    {
                        UseCase = useCaseId,
                        UseCaseName = useCaseName,
                        TestName = $"TC-{testCaseCounter:D2}",
                        Procedure = $"Testing Procedure {procedureIndex} (Main Flow):\n" + string.Join("\n", mainSteps),
                        ExpectedResults = string.Join("\n", mainExpectedResults)
                    };
                    _testCases.Add(mainTestCase);
                    testCaseCounter++;

                    // Sinh Test Cases cho Extensions trong Testing Procedure này
                    for (int i = 0; i < flowOfEvents.Count; i++)
                    {
                        var step = flowOfEvents[i];
                        for (int j = 0; j < step.SubSteps.Count; j++)
                        {
                            var subStep = step.SubSteps[j];
                            foreach (var extension in subStep.Extensions)
                            {
                                var altSteps = new List<string>();
                                var altExpectedResults = new List<string>();
                                int altStepIndex = 1;

                                // Thêm các bước từ đầu đến sub-step hiện tại
                                for (int k = 0; k <= i; k++)
                                {
                                    var currentStep = flowOfEvents[k];
                                    altSteps.Add($"{altStepIndex}. {currentStep.Name}");
                                    altExpectedResults.Add(GenerateExpectedResult(currentStep.Name));
                                    altStepIndex++;

                                    // Thêm sub-steps (nếu có) của bước chính hiện tại
                                    int subStepsToInclude = (k == i) ? j + 1 : currentStep.SubSteps.Count;
                                    for (int m = 0; m < subStepsToInclude; m++)
                                    {
                                        var currentSubStep = currentStep.SubSteps[m];
                                        altSteps.Add($"{altStepIndex}. {currentSubStep.Name}");
                                        altExpectedResults.Add(GenerateExpectedResult(currentSubStep.Name));
                                        altStepIndex++;
                                    }
                                }

                                // Thêm bước kiểm tra với điều kiện extension
                                altSteps.Add($"{altStepIndex}. Hệ thống kiểm tra ({extension.Name}).");
                                altExpectedResults.Add(GenerateErrorMessage(extension.Name));
                                altStepIndex++;

                                // Thêm các bước trong extension
                                foreach (var extStep in extension.SubSteps)
                                {
                                    altSteps.Add($"{altStepIndex}. {extStep.Name}");
                                    altExpectedResults.Add(GenerateExpectedResult(extStep.Name));
                                    altStepIndex++;
                                }

                                var altTestCase = new TestCase
                                {
                                    UseCase = useCaseId,
                                    UseCaseName = useCaseName,
                                    TestName = $"TC-{testCaseCounter:D2}",
                                    Procedure = $"Testing Procedure {procedureIndex} (Alternative Flow):\n" + string.Join("\n", altSteps),
                                    ExpectedResults = string.Join("\n", altExpectedResults)
                                };
                                _testCases.Add(altTestCase);
                                testCaseCounter++;
                            }
                        }
                    }

                    procedureIndex++;
                }

                if (_testCases.Count == 0)
                {
                    txtThongbao.AppendText("Không sinh được test case nào từ Use Case!\r\n");
                }
                else
                {
                    txtThongbao.AppendText($"Đã sinh ra {_testCases.Count} test case từ Use Case!\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi sinh test case từ XMI: {ex.Message}\r\n");
            }
        }


        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hướng dẫn sử dụng:\n1. Chọn định dạng đầu vào (XML, XMI, hoặc Text).\n2. Chọn file đặc tả.\n3. Chọn Use Case từ danh sách (nếu là XML hoặc XMI).\n4. Chọn thư mục đầu ra.\n5. Chọn định dạng đầu ra (Word, Excel, HTML).\n6. Nhấn 'Sinh test case' để tạo test case.\n7. Nhấn 'Xuất báo cáo' để lưu báo cáo (nếu cần).");
        }
                        
    }
}
