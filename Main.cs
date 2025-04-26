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
        private bool needToGenerateTestCase = false;  
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
            public string UseCase { get; set; }
            public string Step { get; set; }
            public string TestName { get; set; }
            public string Preconditions { get; set; }
            public string Procedure { get; set; }
            public string ExpectedResults { get; set; }
            public string Postconditions { get; set; }
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

        private void LoadXmlAndUseCases(string xmlFilePath)
        {

            //try
            //{
            //    _selectedFilePath = xmlFilePath; // Lưu đường dẫn file XML
            //    xmlDoc = XDocument.Load(xmlFilePath); // Tải file XML
            //                                          // Lấy danh sách Use Case từ file XML
            //    var useCases = xmlDoc.Descendants("UseCase")
            //        .Where(uc => uc.Attribute("Id") != null)
            //        .Select(uc => new { Id = uc.Attribute("Id").Value, Name = uc.Attribute("Name").Value })
            //        .ToList();

            //    txtInputTM.Clear(); // Xóa nội dung cũ trong TextBox
            //                        // Hiển thị toàn bộ đường dẫn thư mục chứa file XML
            //    string directoryPath = Path.GetDirectoryName(_selectedFilePath); // Lấy đường dẫn thư mục
            //    txtInputTM.AppendText(directoryPath); // Hiển thị toàn bộ đường dẫn thư mục (giống txtOutputTM)

            //    // Tự động chọn Use Case đầu tiên (ngầm)
            //    if (useCases.Count > 0)
            //    {
            //        var firstUseCase = useCases.First(); // Chọn Use Case đầu tiên
            //        selectedUseCaseId = firstUseCase.Id; // Lưu ID của Use Case đầu tiên
            //                                             // Không hiển thị thông báo để hoàn toàn ngầm
            //    }
            //    else
            //    {
            //        txtThongbao.AppendText("Không tìm thấy Use Case trong file XML!\r\n");
            //        selectedUseCaseId = null; // Đặt lại nếu không có Use Case
            //    }
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi tải file XML: {ex.Message}\r\n");
            //}


            //đẩy dlieu xml lên combobox để chọn tên use case sinh test
            //try
            //{
            //    _selectedFilePath = xmlFilePath; // Lưu đường dẫn file XML
            //    xmlDoc = XDocument.Load(xmlFilePath); // Tải file XML

            //    // Lấy danh sách Use Case từ file XML và loại bỏ trùng lặp dựa trên Name
            //    var useCases = xmlDoc.Descendants("UseCase")
            //        .Where(uc => uc.Attribute("Id") != null)
            //        .Select(uc => new UseCase
            //        {
            //            Id = uc.Attribute("Id").Value,
            //            Name = uc.Attribute("Name").Value
            //        })
            //        .GroupBy(uc => uc.Name) // Nhóm theo Name để loại bỏ trùng lặp
            //        .Select(g => g.First()) // Chọn mục đầu tiên trong mỗi nhóm
            //        .OrderBy(uc => uc.Name) // Sắp xếp theo Name
            //        .ToList();

            //    // Xóa nội dung cũ trong TextBox
            //    txtInputTM.Clear();
            //    // Hiển thị toàn bộ đường dẫn thư mục chứa file XML
            //    string directoryPath = Path.GetDirectoryName(_selectedFilePath);
            //    txtInputTM.AppendText(directoryPath);

            //    // Hiển thị danh sách Use Case trong ComboBox
            //    comboboxUC.Items.Clear(); // Xóa các mục cũ
            //    comboboxUC.Items.AddRange(useCases.ToArray()); // Thêm danh sách use case
            //    comboboxUC.DisplayMember = "Name"; // Hiển thị thuộc tính Name
            //    comboboxUC.ValueMember = "Id"; // Lưu trữ giá trị Id

            //    // Đặt lại selectedUseCaseId và không chọn mặc định
            //    selectedUseCaseId = null;
            //    comboboxUC.SelectedIndex = -1; // Không chọn mục nào mặc định

            //    if (useCases.Count > 0)
            //    {
            //        txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để sinh test case.\r\n");
            //    }
            //    else
            //    {
            //        txtThongbao.AppendText("Không tìm thấy Use Case trong file XML!\r\n");
            //        selectedUseCaseId = null;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi tải file XML: {ex.Message}\r\n");
            //}


            try
            {
                _selectedFilePath = xmlFilePath; // Lưu đường dẫn file XML
                xmlDoc = XDocument.Load(xmlFilePath); // Tải file XML

                // Lấy danh sách Use Case từ file XML và loại bỏ trùng lặp dựa trên Name
                var useCases = xmlDoc.Descendants("UseCase")
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

                // Xóa nội dung cũ trong TextBox
                txtInputTM.Clear();
                // Hiển thị toàn bộ đường dẫn thư mục chứa file XML
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
                    txtThongbao.AppendText("Không tìm thấy Use Case trong file XML!\r\n");
                    selectedUseCaseId = null;
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi tải file XML: {ex.Message}\r\n");
            }
        }


        private List<UseCase> LoadUseCases(string filePath)
        {
            var useCases = new List<UseCase>();
            try
            {
                xmlDoc = XDocument.Load(filePath); // Tải file XML
                                                   // Lấy danh sách Use Case từ file XML
                var xmlUseCases = xmlDoc.Descendants("UseCase")
                    .Where(uc => uc.Attribute("Id") != null)
                    .Select(uc => new UseCase
                    {
                        Id = uc.Attribute("Id").Value,
                        Name = uc.Attribute("Name").Value
                    });

                useCases.AddRange(xmlUseCases); // Thêm các Use Case vào danh sách
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi đọc Use Cases từ file XML: {ex.Message}\r\n");
            }
            return useCases;
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
            
            try
            {
                _testCases.Clear(); // Xóa danh sách test case cũ
                txtThongbao.Text = string.Empty; // Xóa hoàn toàn nội dung thông báo

                // Đọc toàn bộ nội dung file .txt
                string content = File.ReadAllText(txtFilePath, Encoding.UTF8);

                // Lấy thông tin Use Case
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

                // Hỗ trợ cả Preconditions và Pre-Condition(s)
                string preconditions = ExtractValue(content, @"(Pre-Condition\(s\)|Preconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|Post-Condition\(s\)|Postconditions|$))", 2, RegexOptions.IgnoreCase);

                // Hỗ trợ cả Postconditions và Post-Condition(s)
                string postconditions = ExtractValue(content, @"(Post-Condition\(s\)|Postconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|$))", 2, RegexOptions.IgnoreCase);

                // UseCase lấy từ useCaseId (ví dụ: UC_001 hoặc UC-1.1)
                string useCase = string.IsNullOrEmpty(useCaseId) ? "UC-Unknown" : useCaseId;

                // Sinh test case cho BasicFlow hoặc Main Flow
                string basicFlowPattern = @"(BasicFlow|Main Flow|Primary Flow)\s*:([\s\S]*?)(?=(?:ExceptionFlow|Alternative Flows|Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))";
                string basicFlow = ExtractValue(content, basicFlowPattern, 2, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(basicFlow))
                {
                    var testCase = new TestCase
                    {
                        UseCase = useCase,
                        Step = "Main Flow",
                        TestName = "TC-01",
                        Preconditions = preconditions,
                        Procedure = FormatSteps(basicFlow).Replace("Đăng ký", "Đăng nhập"),
                        ExpectedResults = "Hệ thống xác thực thông tin đăng nhập thành công và chuyển đến trang chủ",
                        Postconditions = postconditions
                    };
                    _testCases.Add(testCase);
                }

                // Sinh test case cho ExceptionFlow hoặc Alternative Flows
                string exceptionFlowPattern = @"(ExceptionFlow|Alternative Flows)\s*:([\s\S]*?)(?=(?:Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))";
                string exceptionFlow = ExtractValue(content, exceptionFlowPattern, 2, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(exceptionFlow))
                {
                    // Test case cho ExceptionFlow/Alternative Flow A1 (Đăng nhập không thành công)
                    var testCase1 = new TestCase
                    {
                        UseCase = useCase,
                        Step = "ExceptionFlow A1",
                        TestName = "TC-02",
                        Preconditions = preconditions,
                        Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'"),
                        ExpectedResults = "Hệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'",
                        Postconditions = postconditions
                    };
                    _testCases.Add(testCase1);

                    // Test case cho ExceptionFlow/Alternative Flow A1.1 (Thử lại)
                    var testCase2 = new TestCase
                    {
                        UseCase = useCase,
                        Step = "ExceptionFlow A1.1",
                        TestName = "TC-03",
                        Preconditions = preconditions,
                        Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'\nNgười dùng thử lại"),
                        ExpectedResults = "Người dùng quay lại bước nhập thông tin đăng nhập",
                        Postconditions = postconditions
                    };
                    _testCases.Add(testCase2);

                    // Test case cho ExceptionFlow/Alternative Flow A1.2 (Quên mật khẩu)
                    var testCase3 = new TestCase
                    {
                        UseCase = useCase,
                        Step = "ExceptionFlow A1.2",
                        TestName = "TC-04",
                        Preconditions = preconditions,
                        Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc mật khẩu không đúng\nHệ thống hiển thị thông báo 'Tên đăng nhập hoặc mật khẩu không đúng'\nNgười dùng chọn 'Quên mật khẩu'"),
                        ExpectedResults = "Hệ thống chuyển đến giao diện khôi phục mật khẩu",
                        Postconditions = postconditions
                    };
                    _testCases.Add(testCase3);
                }

                // Sinh test case cho Exceptions (nếu có)
                string exceptionsPattern = @"Exceptions\s*:([\s\S]*?)(?=(?:Post-Condition\(s\)|Postconditions|$))";
                string exceptions = ExtractValue(content, exceptionsPattern, 1, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(exceptions))
                {
                    // Test case cho Exception E1 (Tên đăng nhập hoặc email không tồn tại)
                    if (exceptions.Contains("E1"))
                    {
                        var testCase4 = new TestCase
                        {
                            UseCase = useCase,
                            Step = "Exception E1",
                            TestName = "TC-05",
                            Preconditions = preconditions,
                            Procedure = FormatSteps("Người dùng nhập tên đăng nhập hoặc email không tồn tại"),
                            ExpectedResults = "Hệ thống hiển thị thông báo 'Tên đăng nhập hoặc email không tồn tại trong hệ thống'",
                            Postconditions = postconditions
                        };
                        _testCases.Add(testCase4);
                    }

                    // Test case cho Exception E2 (Lỗi gửi email)
                    if (exceptions.Contains("E2"))
                    {
                        var testCase5 = new TestCase
                        {
                            UseCase = useCase,
                            Step = "Exception E2",
                            TestName = "TC-06",
                            Preconditions = preconditions,
                            Procedure = FormatSteps("Hệ thống gặp lỗi khi gửi email khôi phục"),
                            ExpectedResults = "Hệ thống hiển thị thông báo 'Không thể gửi email khôi phục, vui lòng thử lại sau'",
                            Postconditions = postconditions
                        };
                        _testCases.Add(testCase5);
                    }
                }

                // Hiển thị thông báo khi sinh test case thành công hoặc không sinh được
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
                    openFileDialog.Filter = "XMI files (*.xmi)|*.xmi|All files (*.*)|*.*";
                }
                else
                {
                    openFileDialog.Filter = "Supported files (*.txt;*.xml;*.xmi)|*.txt;*.xml;*.xmi|All files (*.*)|*.*";
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
                        // Xử lý file XMI (chưa triển khai)
                        txtThongbao.AppendText("Chức năng xử lý file XMI chưa được triển khai.\r\n");
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
            if (comboboxUC.SelectedIndex > -1) // Kiểm tra xem có mục nào được chọn không
            {
                selectedUseCaseId = comboboxUC.SelectedValue?.ToString(); // Lấy giá trị Id từ SelectedValue
                if (!string.IsNullOrEmpty(selectedUseCaseId))
                {
                    var selectedUseCase = comboboxUC.SelectedItem as UseCase;
                    txtThongbao.AppendText($"Đã chọn Use Case: {selectedUseCase?.Name}, ID: {selectedUseCaseId}\r\n");
                }
                else
                {
                    selectedUseCaseId = null;
                    txtThongbao.AppendText("Không lấy được ID của Use Case!\r\n");
                }
            }
            else
            {
                selectedUseCaseId = null;
            }
        }


        // Hàm sinh test case từ file XML và lưu vào danh sách _testCases
        private void GenerateTestCases()
        {

            //try
            //{
            //    _testCases.Clear(); // Xóa danh sách test case cũ
            //    txtThongbao.Clear(); // Xóa nội dung cũ trong txtThongbao

            //    // Tìm Use Case trong file XML dựa trên ID
            //    var useCase = xmlDoc.Descendants("UseCase")
            //        .FirstOrDefault(uc => uc.Attribute("Id")?.Value == selectedUseCaseId);

            //    if (useCase == null)
            //    {
            //        txtThongbao.AppendText("Không tìm thấy Use Case!\r\n");
            //        return;
            //    }

            //    string useCaseName = useCase.Attribute("Id")?.Value ?? "UC-Unknown"; // Dùng Id làm UseCase (ví dụ: UC-1.1)

            //    // Lấy thông tin Preconditions và Post-conditions
            //    var taggedValues = useCase.Element("TaggedValues")?.Element("TaggedValueContainer")?.Elements("TaggedValue");
            //    var preconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Preconditions")?.Attribute("Value")?.Value ?? "";
            //    var postconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Post-conditions")?.Attribute("Value")?.Value ?? "";

            //    // Lấy các bước (Steps) và Testing Procedures
            //    var steps = useCase.Element("StepContainers")?.Element("StepContainer")?.Element("Steps")?.Elements("Step");
            //    int stepNumber = 1;
            //    int testCaseCounter = 1; // Đếm để tạo Test Case ID (TC-01, TC-02, v.v.)

            //    if (steps == null)
            //    {
            //        txtThongbao.AppendText("Không tìm thấy bước nào trong Use Case!\r\n");
            //        return;
            //    }

            //    foreach (var step in steps)
            //    {
            //        string stepName = step.Attribute("Name")?.Value ?? $"Step {stepNumber}";

            //        // Lấy Testing Procedures của bước
            //        var testingProcedures = step.Element("TestingProcedures")?.Elements("TestingProcedure");
            //        if (testingProcedures != null)
            //        {
            //            foreach (var tp in testingProcedures)
            //            {
            //                string tpName = $"TC-{testCaseCounter:D2}"; // Tạo Test Case ID dạng TC-01, TC-02, v.v.

            //                // Kiểm tra xem Procedure và ExpectedResults có tồn tại và không bị đánh dấu là null
            //                string procedure = tp.Attribute("Procedure_IsNull")?.Value != "true" ? (tp.Element("Procedure")?.Value ?? $"Step {stepNumber}: {stepName}") : $"Step {stepNumber}: {stepName}";
            //                string expectedResults = tp.Attribute("ExpectedResults_IsNull")?.Value != "true" ? (tp.Element("ExpectedResults")?.Value ?? "Hệ thống xử lý thành công") : "Hệ thống xử lý thành công";

            //                // Sửa Expected Result mơ hồ
            //                if (string.IsNullOrEmpty(expectedResults) || expectedResults == "Hệ thống xử lý thông tin đăng nhập")
            //                {
            //                    expectedResults = "Hệ thống chuyển hướng đến trang thông tin tài khoản";
            //                }
            //                else if (expectedResults == "Hệ thống xử lý yêu cầu đặt lại mật khẩu")
            //                {
            //                    expectedResults = "Hệ thống gửi email chứa liên kết đặt lại mật khẩu";
            //                }

            //                // Tạo một TestCase và thêm vào danh sách _testCases
            //                var testCase = new TestCase
            //                {
            //                    UseCase = useCaseName,
            //                    Step = $"Step {stepNumber}",
            //                    TestName = tpName,
            //                    Preconditions = preconditions,
            //                    Procedure = procedure,
            //                    ExpectedResults = expectedResults,
            //                    Postconditions = postconditions
            //                };
            //                _testCases.Add(testCase);
            //                testCaseCounter++;
            //            }
            //        }

            //        // Kiểm tra các bước con (Sub-steps) nếu có
            //        var subSteps = step.Element("SubSteps")?.Elements("Step");
            //        if (subSteps != null)
            //        {
            //            int subStepNumber = 1;
            //            foreach (var subStep in subSteps)
            //            {
            //                string subStepName = subStep.Attribute("Name")?.Value ?? $"Sub-step {subStepNumber}";

            //                var subTestingProcedures = subStep.Element("TestingProcedures")?.Elements("TestingProcedure");
            //                if (subTestingProcedures != null)
            //                {
            //                    foreach (var subTp in subTestingProcedures)
            //                    {
            //                        string subTpName = $"TC-{testCaseCounter:D2}";
            //                        string subProcedure = subTp.Attribute("Procedure_IsNull")?.Value != "true" ? (subTp.Element("Procedure")?.Value ?? $"Sub-step {stepNumber}.{subStepNumber}: {subStepName}") : $"Sub-step {stepNumber}.{subStepNumber}: {subStepName}";
            //                        string subExpectedResults = subTp.Attribute("ExpectedResults_IsNull")?.Value != "true" ? (subTp.Element("ExpectedResults")?.Value ?? "Hệ thống xử lý thành công") : "Hệ thống xử lý thành công";

            //                        // Sửa Expected Result mơ hồ
            //                        if (string.IsNullOrEmpty(subExpectedResults) || subExpectedResults == "Hệ thống xử lý thông tin đăng nhập")
            //                        {
            //                            subExpectedResults = "Hệ thống chuyển hướng đến trang thông tin tài khoản";
            //                        }
            //                        else if (subExpectedResults == "Hệ thống xử lý yêu cầu đặt lại mật khẩu")
            //                        {
            //                            subExpectedResults = "Hệ thống gửi email chứa liên kết đặt lại mật khẩu";
            //                        }

            //                        // Tạo một TestCase cho Sub-step
            //                        var testCase = new TestCase
            //                        {
            //                            UseCase = useCaseName,
            //                            Step = $"Sub-step {stepNumber}.{subStepNumber}",
            //                            TestName = subTpName,
            //                            Preconditions = preconditions,
            //                            Procedure = subProcedure,
            //                            ExpectedResults = subExpectedResults,
            //                            Postconditions = postconditions
            //                        };
            //                        _testCases.Add(testCase);
            //                        testCaseCounter++;
            //                    }
            //                }
            //                subStepNumber++;
            //            }
            //        }

            //        // Kiểm tra Extensions (luồng phụ như Quên mật khẩu)
            //        var extensions = step.Element("Extensions")?.Elements("Extension");
            //        if (extensions != null)
            //        {
            //            Dictionary<string, TestCase> extensionTestCases = new Dictionary<string, TestCase>(); // Để kiểm tra trùng lặp

            //            foreach (var extension in extensions)
            //            {
            //                string extName = extension.Attribute("Name")?.Value ?? "Unnamed Extension";
            //                var extSteps = extension.Element("SubSteps")?.Elements("Step");
            //                int extStepNumber = 1;

            //                if (extSteps != null)
            //                {
            //                    foreach (var extStep in extSteps)
            //                    {
            //                        string extStepName = extStep.Attribute("Name")?.Value ?? $"Ext Step {extStepNumber}";
            //                        var extTestingProcedures = extStep.Element("TestingProcedures")?.Elements("TestingProcedure");

            //                        if (extTestingProcedures != null)
            //                        {
            //                            foreach (var extTp in extTestingProcedures)
            //                            {
            //                                string extTpName = $"TC-{testCaseCounter:D2}";
            //                                string extProcedure = extTp.Attribute("Procedure_IsNull")?.Value != "true" ? (extTp.Element("Procedure")?.Value ?? $"Extension {extName} - Step {extStepNumber}: {extStepName}") : $"Extension {extName} - Step {extStepNumber}: {extStepName}";
            //                                string extExpectedResults = extTp.Attribute("ExpectedResults_IsNull")?.Value != "true" ? (extTp.Element("ExpectedResults")?.Value ?? "Hệ thống xử lý thành công") : "Hệ thống xử lý thành công";

            //                                // Sửa Expected Result mơ hồ
            //                                if (string.IsNullOrEmpty(extExpectedResults) || extExpectedResults == "Hệ thống xử lý thông tin đăng nhập")
            //                                {
            //                                    extExpectedResults = "Hệ thống chuyển hướng đến trang thông tin tài khoản";
            //                                }
            //                                else if (extExpectedResults == "Hệ thống xử lý yêu cầu đặt lại mật khẩu")
            //                                {
            //                                    extExpectedResults = "Hệ thống gửi email chứa liên kết đặt lại mật khẩu";
            //                                }

            //                                // Kiểm tra trùng lặp dựa trên Procedure và Expected Result
            //                                string key = extProcedure + extExpectedResults;
            //                                if (!extensionTestCases.ContainsKey(key))
            //                                {
            //                                    var testCase = new TestCase
            //                                    {
            //                                        UseCase = useCaseName,
            //                                        Step = $"Extension {extName} - Step {extStepNumber}",
            //                                        TestName = extTpName,
            //                                        Preconditions = preconditions,
            //                                        Procedure = extProcedure,
            //                                        ExpectedResults = extExpectedResults,
            //                                        Postconditions = postconditions
            //                                    };
            //                                    extensionTestCases[key] = testCase;
            //                                    _testCases.Add(testCase);
            //                                    testCaseCounter++;
            //                                }
            //                            }
            //                        }
            //                        extStepNumber++;
            //                    }
            //                }
            //            }
            //        }

            //        stepNumber++;
            //    }


            //    // Loại bỏ test case trống
            //    _testCases.RemoveAll(tc => string.IsNullOrEmpty(tc.Procedure) && string.IsNullOrEmpty(tc.ExpectedResults));

            //    if (_testCases.Count == 0)
            //    {
            //        txtThongbao.AppendText("Không sinh được test case nào từ Use Case!\r\n");
            //    }
            //    else
            //    {
            //        txtThongbao.AppendText($"Đã sinh ra {_testCases.Count} test case từ Use Case!\r\n");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi sinh test case: {ex.Message}\r\n");
            //}


            //SINH TC
            try
            {
                _testCases.Clear(); // Xóa danh sách test case cũ
                txtThongbao.Clear(); // Xóa nội dung cũ trong txtThongbao

                // Tìm Use Case trong file XML dựa trên ID
                var useCase = xmlDoc.Descendants("UseCase")
                    .FirstOrDefault(uc => uc.Attribute("Id")?.Value == selectedUseCaseId);

                if (useCase == null)
                {
                    txtThongbao.AppendText("Không tìm thấy Use Case!\r\n");
                    return;
                }

                string useCaseName = useCase.Attribute("Name")?.Value ?? "Unknown Use Case"; // Lấy tên Use Case (ví dụ: 1.1. Đăng nhập)
                string useCaseId = useCase.Attribute("Id")?.Value ?? "UC-Unknown"; // Lưu Id để sử dụng nếu cần

                // Lấy thông tin Preconditions và Post-conditions
                var taggedValues = useCase.Element("TaggedValues")?.Element("TaggedValueContainer")?.Elements("TaggedValue");
                var preconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Preconditions")?.Attribute("Value")?.Value ?? "";
                var postconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Post-conditions")?.Attribute("Value")?.Value ?? "";

                // Lấy các bước (Steps)
                var stepContainer = useCase.Element("StepContainers")?.Element("StepContainer");
                var steps = stepContainer?.Element("Steps")?.Elements("Step");
                if (steps == null)
                {
                    txtThongbao.AppendText("Không tìm thấy bước nào trong Use Case!\r\n");
                    return;
                }

                int tcCounter = 1; // Đếm để tạo Test Case ID (TC-01, TC-02, ...)

                // Tạo danh sách các luồng
                List<List<string>> mediumFlows = new List<List<string>>();
                List<List<string>> googleFlows = new List<List<string>>();
                List<List<string>> facebookFlows = new List<List<string>>();
                List<(string ExtName, List<string> ExtSteps)> extensionFlows = new List<(string, List<string>)>();

                // Phân loại các bước vào từng luồng
                List<string> currentMediumFlow = new List<string>();
                List<string> currentGoogleFlow = new List<string>();
                List<string> currentFacebookFlow = new List<string>();
                bool isMediumFlow = false, isGoogleFlow = false, isFacebookFlow = false;

                foreach (var step in steps)
                {
                    string stepName = step.Attribute("Name")?.Value ?? $"Step {mediumFlows.Count + googleFlows.Count + facebookFlows.Count + 1}";

                    // Bước chung: "Người dùng truy cập ứng dụng Medium"
                    if (stepName.ToLower().Contains("truy cập ứng dụng"))
                    {
                        if (currentMediumFlow.Any()) mediumFlows.Add(new List<string>(currentMediumFlow));
                        if (currentGoogleFlow.Any()) googleFlows.Add(new List<string>(currentGoogleFlow));
                        if (currentFacebookFlow.Any()) facebookFlows.Add(new List<string>(currentFacebookFlow));

                        currentMediumFlow = new List<string> { stepName };
                        currentGoogleFlow = new List<string> { stepName };
                        currentFacebookFlow = new List<string> { stepName };
                        isMediumFlow = isGoogleFlow = isFacebookFlow = false;
                        continue;
                    }

                    // Xác định luồng dựa trên nội dung bước
                    if (stepName.ToLower().Contains("chọn phương thức đăng nhập bằng tài khoản medium"))
                    {
                        isMediumFlow = true;
                        isGoogleFlow = isFacebookFlow = false;
                        currentMediumFlow.Add(stepName);
                    }
                    else if (stepName.ToLower().Contains("chọn phương thức đăng nhập bằng tài khoản google"))
                    {
                        isGoogleFlow = true;
                        isMediumFlow = isFacebookFlow = false;
                        currentGoogleFlow.Add(stepName);
                    }
                    else if (stepName.ToLower().Contains("chọn phương thức đăng nhập bằng tài khoản facebook"))
                    {
                        isFacebookFlow = true;
                        isMediumFlow = isGoogleFlow = false;
                        currentFacebookFlow.Add(stepName);
                    }
                    else if (stepName.ToLower().Contains("google xác thực"))
                    {
                        currentGoogleFlow.Add(stepName);
                    }
                    else if (stepName.ToLower().Contains("facebook xác thực"))
                    {
                        currentFacebookFlow.Add(stepName);
                    }
                    else if (stepName.ToLower().Contains("nhập tài khoản medium") || stepName.ToLower().Contains("hệ thống xác thực thông tin đăng nhập"))
                    {
                        currentMediumFlow.Add(stepName);
                    }

                    // Kiểm tra Extensions (luồng phụ)
                    var extensions = step.Element("Extensions")?.Elements("Extension");
                    if (extensions != null && extensions.Any())
                    {
                        foreach (var extension in extensions)
                        {
                            string extName = extension.Attribute("Name")?.Value ?? "Unnamed Extension";
                            List<string> extSteps = new List<string>();

                            // Thêm các bước trước đó của Medium Flow vào Extension
                            extSteps.AddRange(currentMediumFlow);

                            // Lấy các bước của Extension
                            var extStepElements = extension.Element("Steps")?.Elements("Step");
                            if (extStepElements != null)
                            {
                                foreach (var extStep in extStepElements)
                                {
                                    string extStepName = extStep.Attribute("Name")?.Value ?? "";
                                    if (!string.IsNullOrEmpty(extStepName))
                                    {
                                        extSteps.Add(extStepName);
                                    }
                                }
                            }

                            extensionFlows.Add((extName, extSteps));
                        }
                    }
                }

                // Lưu các luồng cuối cùng nếu còn
                if (currentMediumFlow.Count > 1) mediumFlows.Add(currentMediumFlow);
                if (currentGoogleFlow.Count > 1) googleFlows.Add(currentGoogleFlow);
                if (currentFacebookFlow.Count > 1) facebookFlows.Add(currentFacebookFlow);

                // Tạo Test Case cho Medium Flows (TC-XX)
                foreach (var flow in mediumFlows)
                {
                    if (flow.Count > 0)
                    {
                        // Test Case thành công (không Activity Log)
                        string procedure = string.Join("\r\n", flow);
                        string expectedResults = "Hệ thống chuyển đến trang chủ";

                        var testCase = new TestCase
                        {
                            UseCase = useCaseName,
                            TestName = $"TC-{tcCounter:D2}",
                            Procedure = procedure,
                            ExpectedResults = expectedResults
                        };
                        _testCases.Add(testCase);
                        tcCounter++;

                        // Test Case thành công (có Activity Log)
                        List<string> flowWithLog = new List<string>(flow);
                        if (flow.Any(step => step.ToLower().Contains("xác thực thông tin đăng nhập thành công")))
                        {
                            flowWithLog.Add("Hệ thống ghi nhận hoạt động đăng nhập thành công vào Activity Log");
                        }
                        string procedureWithLog = string.Join("\r\n", flowWithLog);

                        var testCaseWithLog = new TestCase
                        {
                            UseCase = useCaseName,
                            TestName = $"TC-{tcCounter:D2}",
                            Procedure = procedureWithLog,
                            ExpectedResults = expectedResults
                        };
                        _testCases.Add(testCaseWithLog);
                        tcCounter++;

                        // Test Case thất bại
                        if (flow.Any(step => step.ToLower().Contains("nhập tài khoản medium")))
                        {
                            List<string> failureFlow = new List<string>(flow);
                            failureFlow[failureFlow.Count - 1] = "Hệ thống xác thực thông tin đăng nhập không thành công";
                            string failureProcedure = string.Join("\r\n", failureFlow);
                            var failureTestCase = new TestCase
                            {
                                UseCase = useCaseName,
                                TestName = $"TC-{tcCounter:D2}",
                                Procedure = failureProcedure,
                                ExpectedResults = "Hệ thống hiển thị thông báo lỗi"
                            };
                            _testCases.Add(failureTestCase);
                            tcCounter++;
                        }
                    }
                }

                // Tạo Test Case cho Google Flows (TC-XX)
                foreach (var flow in googleFlows)
                {
                    if (flow.Count > 0)
                    {
                        List<string> flowWithLog = new List<string>(flow);
                        if (flow.Any(step => step.ToLower().Contains("xác thực thông tin đăng nhập thành công")))
                        {
                            flowWithLog.Add("Hệ thống ghi nhận hoạt động đăng nhập thành công vào Activity Log");
                        }

                        string procedureWithLog = string.Join("\r\n", flowWithLog);
                        string expectedResults = "Hệ thống chuyển đến trang chủ";

                        var testCaseWithLog = new TestCase
                        {
                            UseCase = useCaseName,
                            TestName = $"TC-{tcCounter:D2}",
                            Procedure = procedureWithLog,
                            ExpectedResults = expectedResults
                        };
                        _testCases.Add(testCaseWithLog);
                        tcCounter++;
                    }
                }

                // Tạo Test Case cho Facebook Flows (TC-XX)
                foreach (var flow in facebookFlows)
                {
                    if (flow.Count > 0)
                    {
                        List<string> flowWithLog = new List<string>(flow);
                        if (flow.Any(step => step.ToLower().Contains("xác thực thông tin đăng nhập thành công")))
                        {
                            flowWithLog.Add("Hệ thống ghi nhận hoạt động đăng nhập thành công vào Activity Log");
                        }

                        string procedureWithLog = string.Join("\r\n", flowWithLog);
                        string expectedResults = "Hệ thống chuyển đến trang chủ";

                        var testCaseWithLog = new TestCase
                        {
                            UseCase = useCaseName,
                            TestName = $"TC-{tcCounter:D2}",
                            Procedure = procedureWithLog,
                            ExpectedResults = expectedResults
                        };
                        _testCases.Add(testCaseWithLog);
                        tcCounter++;
                    }
                }

                // Tạo Test Case cho Extension Flows (TC-XX)
                foreach (var (extName, extSteps) in extensionFlows)
                {
                    if (extSteps.Count > 0)
                    {
                        string procedure = string.Join("\r\n", extSteps);
                        string expectedResults = extName.ToLower().Contains("quên mật khẩu") ? "Hệ thống gửi thông tin đặt lại mật khẩu" : "Thao tác ngoại lệ hoàn tất";

                        var testCase = new TestCase
                        {
                            UseCase = useCaseName,
                            TestName = $"TC-{tcCounter:D2}",
                            Procedure = procedure,
                            ExpectedResults = expectedResults
                        };
                        _testCases.Add(testCase);
                        tcCounter++;
                    }
                }

                // Kiểm tra nếu không có test case nào được tạo
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





            //SINH TEST CASE KHI FILE XML CÓ HIỆN RÕ BASIC FLOW, ...
            //try
            //{
            //    _testCases.Clear(); // Xóa danh sách test case cũ
            //    txtThongbao.Clear(); // Xóa nội dung cũ trong txtThongbao

            //    // Tìm Use Case trong file XML dựa trên ID
            //    var useCase = xmlDoc.Descendants("UseCase")
            //        .FirstOrDefault(uc => uc.Attribute("Id")?.Value == selectedUseCaseId);

            //    if (useCase == null)
            //    {
            //        txtThongbao.AppendText("Không tìm thấy Use Case!\r\n");
            //        return;
            //    }

            //    string useCaseName = useCase.Attribute("Name")?.Value ?? "UC-Unknown"; // Sử dụng Name thay vì Id

            //    // Lấy thông tin Preconditions và Post-conditions
            //    var taggedValues = useCase.Element("TaggedValues")?.Elements("TaggedValue");
            //    var preconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Preconditions")?.Attribute("Value")?.Value ?? "";
            //    var postconditions = taggedValues?.FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Post-conditions")?.Attribute("Value")?.Value ?? "";

            //    int testCaseCounter = 1; // Đếm để tạo Test Case ID (TC-01, TC-02, v.v.)

            //    // Lấy các luồng (Flows)
            //    var flows = useCase.Element("Flows");
            //    if (flows == null)
            //    {
            //        txtThongbao.AppendText("Không tìm thấy luồng nào trong Use Case!\r\n");
            //        return;
            //    }

            //    // **Sinh test case cho UC-1.1 (Đăng nhập)**

            //    var basicFlow = flows.Element("BasicFlow");
            //    if (basicFlow == null)
            //    {
            //        txtThongbao.AppendText("Không tìm thấy Basic Flow trong Use Case!\r\n");
            //        return;
            //    }

            //    // Test Case 1: Mở ứng dụng và chọn phương thức đăng nhập (bước 1 và 2 của Basic Flow)
            //    var stepsTC1 = basicFlow.Elements("Step")
            //        .Where(s => new[] { "Step 1", "Step 2" }.Contains(s.Attribute("Name")?.Value))
            //        .Select(s => s.Attribute("Description")?.Value)
            //        .Where(desc => !string.IsNullOrEmpty(desc));
            //    var procedureTC1 = string.Join("\n", stepsTC1);
            //    var expectedResultTC1 = basicFlow.Elements("Step")
            //        .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 1")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống chuyển đến trang chủ";

            //    var testCase1 = new TestCase
            //    {
            //        UseCase = useCaseName,
            //        TestName = $"TC-{testCaseCounter:D2}",
            //        Preconditions = preconditions,
            //        Procedure = procedureTC1,
            //        ExpectedResults = expectedResultTC1,


            //        Postconditions = postconditions
            //    };
            //    _testCases.Add(testCase1);
            //    testCaseCounter++;

            //    // Test Case 2: Đăng nhập bằng tài khoản Medium (Basic Flow đầy đủ)
            //    var stepsTC2 = basicFlow.Elements("Step")
            //        .Select(s => s.Attribute("Description")?.Value)
            //        .Where(desc => !string.IsNullOrEmpty(desc));
            //    var procedureTC2 = string.Join("\n", stepsTC2);
            //    var expectedResultTC2 = basicFlow.Elements("Step")
            //        .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 4")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống chuyển đến trang bài viết";

            //    var testCase2 = new TestCase
            //    {
            //        UseCase = useCaseName,
            //        TestName = $"TC-{testCaseCounter:D2}",
            //        Preconditions = preconditions,
            //        Procedure = procedureTC2,
            //        ExpectedResults = expectedResultTC2,
            //        Postconditions = postconditions
            //    };
            //    _testCases.Add(testCase2);
            //    testCaseCounter++;

            //    // Test Case 3: Đăng nhập bằng Gmail (Alternative Flow 2a + bước 5 của Basic Flow)
            //    var altFlowGmail = flows.Elements("AlternativeFlow").FirstOrDefault(f => f.Attribute("Name")?.Value == "2a");
            //    if (altFlowGmail != null)
            //    {
            //        var stepsGmail = new List<string>
            //{
            //    basicFlow.Elements("Step").FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 1")?.Attribute("Description")?.Value
            //};
            //        stepsGmail.AddRange(altFlowGmail.Elements("Step")
            //            .Select(s => s.Attribute("Description")?.Value)
            //            .Where(desc => !string.IsNullOrEmpty(desc)));
            //        stepsGmail.Add(basicFlow.Elements("Step")
            //            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 5")?.Attribute("Description")?.Value);

            //        var procedureGmail = string.Join("\n", stepsGmail.Where(s => !string.IsNullOrEmpty(s)));
            //        var expectedResultGmail = altFlowGmail.Elements("Step")
            //            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 4a")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống chuyển đến trang bài viết";

            //        var testCase3 = new TestCase
            //        {
            //            UseCase = useCaseName,
            //            TestName = $"TC-{testCaseCounter:D2}",
            //            Preconditions = preconditions,
            //            Procedure = procedureGmail,
            //            ExpectedResults = expectedResultGmail,
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase3);
            //        testCaseCounter++;
            //    }

            //    // Test Case 4: Đăng nhập bằng Facebook (Alternative Flow 2b + bước 5 của Basic Flow)
            //    var altFlowFacebook = flows.Elements("AlternativeFlow").FirstOrDefault(f => f.Attribute("Name")?.Value == "2b");
            //    if (altFlowFacebook != null)
            //    {
            //        var stepsFacebook = new List<string>
            //{
            //    basicFlow.Elements("Step").FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 1")?.Attribute("Description")?.Value
            //};
            //        stepsFacebook.AddRange(altFlowFacebook.Elements("Step")
            //            .Select(s => s.Attribute("Description")?.Value)
            //            .Where(desc => !string.IsNullOrEmpty(desc)));
            //        stepsFacebook.Add(basicFlow.Elements("Step")
            //            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 5")?.Attribute("Description")?.Value);

            //        var procedureFacebook = string.Join("\n", stepsFacebook.Where(s => !string.IsNullOrEmpty(s)));
            //        var expectedResultFacebook = altFlowFacebook.Elements("Step")
            //            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 4b")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống chuyển đến trang bài viết";

            //        var testCase4 = new TestCase
            //        {
            //            UseCase = useCaseName,
            //            TestName = $"TC-{testCaseCounter:D2}",
            //            Preconditions = preconditions,
            //            Procedure = procedureFacebook,
            //            ExpectedResults = expectedResultFacebook,
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase4);
            //        testCaseCounter++;
            //    }

            //    // Test Case 5: Đăng nhập thất bại (Exception Flow 4c)
            //    var excFlow = flows.Element("ExceptionFlow");
            //    if (excFlow != null)
            //    {
            //        var stepsFail = new List<string>
            //{
            //    basicFlow.Elements("Step").FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 1")?.Attribute("Description")?.Value,
            //    basicFlow.Elements("Step").FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 2")?.Attribute("Description")?.Value
            //};
            //        stepsFail.AddRange(excFlow.Elements("Step")
            //            .Select(s => s.Attribute("Description")?.Value)
            //            .Where(desc => !string.IsNullOrEmpty(desc)));

            //        var procedureFail = string.Join("\n", stepsFail.Where(s => !string.IsNullOrEmpty(s)));
            //        var expectedResultFail = excFlow.Elements("Step")
            //            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 4c")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống hiển thị thông báo lỗi";

            //        var testCase5 = new TestCase
            //        {
            //            UseCase = useCaseName,
            //            TestName = $"TC-{testCaseCounter:D2}",
            //            Preconditions = preconditions,
            //            Procedure = procedureFail,
            //            ExpectedResults = expectedResultFail,
            //            Postconditions = postconditions
            //        };
            //        _testCases.Add(testCase5);
            //        testCaseCounter++;
            //    }

            //    // **Sinh test case cho UC-1.2 (Lấy lại mật khẩu)**

            //    // Tìm các Use Case mở rộng (Extend) từ UC-1.1
            //    var relationships = xmlDoc.Descendants("RelationshipContainer")?.Elements("Relationship")
            //        .Where(r => r.Attribute("Type")?.Value == "Extend" && r.Element("Source")?.Attribute("Id")?.Value == selectedUseCaseId);

            //    if (relationships != null)
            //    {
            //        foreach (var relationship in relationships)
            //        {
            //            string targetUseCaseId = relationship.Element("Target")?.Attribute("Id")?.Value;
            //            var targetUseCase = xmlDoc.Descendants("UseCase")
            //                .FirstOrDefault(uc => uc.Attribute("Id")?.Value == targetUseCaseId);

            //            if (targetUseCase != null)
            //            {
            //                string targetUseCaseName = targetUseCase.Attribute("Name")?.Value ?? "UC-Unknown";
            //                var targetPreconditions = targetUseCase.Element("TaggedValues")?.Elements("TaggedValue")
            //                    .FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Preconditions")?.Attribute("Value")?.Value ?? "";
            //                var targetPostconditions = targetUseCase.Element("TaggedValues")?.Elements("TaggedValue")
            //                    .FirstOrDefault(tv => tv.Attribute("Name")?.Value == "Post-conditions")?.Attribute("Value")?.Value ?? "";

            //                var targetFlows = targetUseCase.Element("Flows");
            //                if (targetFlows != null)
            //                {
            //                    var targetBasicFlow = targetFlows.Element("BasicFlow");
            //                    if (targetBasicFlow != null)
            //                    {
            //                        // Test Case 1 cho UC-1.2: Chọn "Quên mật khẩu"
            //                        var stepsTC6 = targetBasicFlow.Elements("Step")
            //                            .Where(s => s.Attribute("Name")?.Value == "Step 1")
            //                            .Select(s => s.Attribute("Description")?.Value)
            //                            .Where(desc => !string.IsNullOrEmpty(desc));
            //                        var procedureTC6 = string.Join("\n", stepsTC6);
            //                        var expectedResultTC6 = targetBasicFlow.Elements("Step")
            //                            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 1")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống hiển thị form nhập email";

            //                        var testCase6 = new TestCase
            //                        {
            //                            UseCase = targetUseCaseName,
            //                            TestName = $"TC-{testCaseCounter:D2}",
            //                            Preconditions = targetPreconditions,
            //                            Procedure = procedureTC6,
            //                            ExpectedResults = expectedResultTC6,
            //                            Postconditions = targetPostconditions
            //                        };
            //                        _testCases.Add(testCase6);
            //                        testCaseCounter++;

            //                        // Test Case 2 cho UC-1.2: Nhập email và xác nhận
            //                        var stepsTC7 = targetBasicFlow.Elements("Step")
            //                            .Where(s => new[] { "Step 2", "Step 3" }.Contains(s.Attribute("Name")?.Value))
            //                            .Select(s => s.Attribute("Description")?.Value)
            //                            .Where(desc => !string.IsNullOrEmpty(desc));
            //                        var procedureTC7 = string.Join("\n", stepsTC7);
            //                        var expectedResultTC7 = targetBasicFlow.Elements("Step")
            //                            .FirstOrDefault(s => s.Attribute("Name")?.Value == "Step 3")?.Attribute("ExpectedResult")?.Value ?? "Hệ thống gửi email khôi phục mật khẩu";

            //                        var testCase7 = new TestCase
            //                        {
            //                            UseCase = targetUseCaseName,
            //                            TestName = $"TC-{testCaseCounter:D2}",
            //                            Preconditions = targetPreconditions,
            //                            Procedure = procedureTC7,
            //                            ExpectedResults = expectedResultTC7,
            //                            Postconditions = targetPostconditions
            //                        };
            //                        _testCases.Add(testCase7);
            //                        testCaseCounter++;
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    // Hiển thị thông báo
            //    if (_testCases.Count == 0)
            //    {
            //        txtThongbao.AppendText("Không sinh được test case nào từ Use Case!\r\n");
            //    }
            //    else
            //    {
            //        txtThongbao.AppendText($"Đã sinh ra {_testCases.Count} test case từ Use Case!\r\n");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi sinh test case: {ex.Message}\r\n");
            //}
        }

        //phương pháp phân tích cú pháp XML chung
        private void ParseGenericXml(string filePath)
        {
            try
            {
                _testCases.Clear(); // Xóa danh sách test case cũ
                txtThongbao.Clear(); // Xóa nội dung cũ trong txtThongbao

                xmlDoc = XDocument.Load(filePath); // Tải file XML

                // Tìm các phần tử chính (có thể là "UseCase", "TestCase", "Scenario", v.v.)
                var mainElements = xmlDoc.Descendants()
                    .Where(e => e.Name.LocalName.Contains("UseCase") || e.Name.LocalName.Contains("TestCase") || e.Name.LocalName.Contains("Scenario"))
                    .ToList();

                if (!mainElements.Any())
                {
                    txtThongbao.AppendText("Không tìm thấy phần tử chính nào (UseCase/TestCase/Scenario) trong file XML!\r\n");
                    return;
                }

                int testCaseCounter = 1;

                foreach (var element in mainElements)
                {
                    string elementId = element.Attribute("Id")?.Value ?? $"Element-{testCaseCounter}";
                    string elementName = element.Attribute("Name")?.Value ?? "Unnamed Element";

                    // Lấy các điều kiện trước và sau (nếu có)
                    string preconditions = element.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName.Contains("Precondition") || e.Name.LocalName.Contains("Pre-condition"))?.Value
                        ?? element.Attributes().FirstOrDefault(a => a.Name.LocalName.Contains("Precondition") || a.Name.LocalName.Contains("Pre-condition"))?.Value
                        ?? "";
                    string postconditions = element.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName.Contains("Postcondition") || e.Name.LocalName.Contains("Post-condition"))?.Value
                        ?? element.Attributes().FirstOrDefault(a => a.Name.LocalName.Contains("Postcondition") || a.Name.LocalName.Contains("Post-condition"))?.Value
                        ?? "";

                    // Tìm các bước hoặc hành động
                    var steps = element.Descendants()
                        .Where(e => e.Name.LocalName.Contains("Step") || e.Name.LocalName.Contains("Action") || e.Name.LocalName.Contains("Procedure"))
                        .ToList();

                    int stepNumber = 1;
                    foreach (var step in steps)
                    {
                        string stepName = step.Attribute("Name")?.Value ?? step.Value ?? $"Step {stepNumber}";
                        string procedure = step.Descendants()
                            .FirstOrDefault(e => e.Name.LocalName.Contains("Procedure") || e.Name.LocalName.Contains("Action"))?.Value
                            ?? step.Attributes().FirstOrDefault(a => a.Name.LocalName.Contains("Procedure") || a.Name.LocalName.Contains("Action"))?.Value
                            ?? stepName;
                        string expectedResults = step.Descendants()
                            .FirstOrDefault(e => e.Name.LocalName.Contains("ExpectedResult") || e.Name.LocalName.Contains("Expected"))?.Value
                            ?? step.Attributes().FirstOrDefault(a => a.Name.LocalName.Contains("ExpectedResult") || a.Name.LocalName.Contains("Expected"))?.Value
                            ?? "Hệ thống xử lý thành công";

                        // Tạo test case
                        var testCase = new TestCase
                        {
                            UseCase = elementId,
                            Step = $"Step {stepNumber}",
                            TestName = $"TC-{testCaseCounter:D2}",
                            Preconditions = preconditions,
                            Procedure = procedure,
                            ExpectedResults = expectedResults,
                            Postconditions = postconditions
                        };
                        _testCases.Add(testCase);
                        testCaseCounter++;
                        stepNumber++;
                    }
                }

                if (_testCases.Count == 0)
                {
                    txtThongbao.AppendText("Không sinh được test case nào từ file XML!\r\n");
                }
                else
                {
                    txtThongbao.AppendText($"Đã sinh ra {_testCases.Count} test case từ file XML!\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi phân tích file XML: {ex.Message}\r\n");
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
            //        if (string.IsNullOrEmpty(selectedUseCaseId))
            //        {
            //            txtThongbao.AppendText("Không có Use Case nào được chọn (ngầm)!\r\n");
            //            return;
            //        }

            //        // Dùng phương thức cụ thể cho XML hiện tại
            //        GenerateTestCases();
            //        // Nếu muốn thử generic parser, có thể gọi:
            //        // ParseGenericXml(_selectedFilePath);
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
            // Kiểm tra xem đã chọn file và thư mục chưa
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
                    txtThongbao.AppendText("Chức năng xử lý file XMI chưa được triển khai.\r\n");
                    return;
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
                html.AppendLine("<th>Use Case</th><th>Test Case</th><th>Procedure</th><th>Expected Result</th></tr>");

                foreach (var tc in testCases)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{tc.UseCase}</td>");
                    html.AppendLine($"<td>{tc.TestName}</td>");
                    html.AppendLine($"<td>{tc.Procedure.Replace("\n", "<br>")}</td>");
                    html.AppendLine($"<td>{tc.ExpectedResults}</td>");
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
            //txtInputTM.Clear();
            //txtOutputTM.Clear();
            //txtThongbao.Clear();
            //_testCases.Clear();
            //_selectedFilePath = null;
            //selectedUseCaseId = null;
            //radioTextIn.Checked = false;
            //radioXMLIn.Checked = true; // Mặc định chọn XML
            //radioXMIIn.Checked = false;
            //radioWordOut.Checked = true; // Mặc định chọn Word
            //radioExcelOut.Checked = false;
            //radioHTMLOut.Checked = false;
            //txtThongbao.AppendText("Đã làm mới các lựa chọn.\r\n");


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
        

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hướng dẫn sử dụng:\n1. Chọn định dạng đầu vào (XML).\n2. Chọn Use Case từ danh sách.\n3. Chọn thư mục đầu ra.\n4. Nhấn 'Sinh test case' để tạo test case.");
        }





        
    }
}
