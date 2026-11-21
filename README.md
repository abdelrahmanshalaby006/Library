# 📚 City Library — نظام إدارة مكتبة (ASP.NET Core MVC · 3 طبقات · Identity)

الشرح كله بالعربي المبسّط. ابدأ من **"تشغيل المشروع"** وبعدين اقرأ الباقي وقت ما تحب.

---

## 1) تشغيل المشروع (5 خطوات)

**المتطلبات:** Visual Studio 2022/2026 + SQL Server Express + **.NET 10 SDK**
(اتأكد بالأمر `dotnet --list-sdks` لازم تلاقي سطر بيبدأ بـ `10.`).
> لو معندكش .NET 10: غيّر `net10.0` لـ `net9.0` و `10.0.*` لـ `9.0.*` في ملفات الـ 3 `.csproj` وخلاص.

1. **امسح الداتابيز القديمة** (لو كنت جربت النسخة اللي فاتت) — الداتا كانت تجريبية. من Package Manager Console:
   ```
   Drop-Database -Project Library.DAL -StartupProject Library.PL
   ```
   (لو قالك إنها مش موجودة، عادي كمّل.) أو من SSMS امسح `LibraryDb`.
2. افتح `Library.sln`. لو اسم السيرفر مش `.\SQLEXPRESS` عدّله في `Library.PL/appsettings.json`.
3. اعمل Build مرة (Ctrl+Shift+B) وتأكد مفيش Errors.
4. من **Package Manager Console**: (Default project في الـ dropdown أي حاجة، إحنا بنحددهم في الأمر)
   ```
   Add-Migration InitCreate -Project Library.DAL -StartupProject Library.PL
   ```
   ده بيعمل فولدر `Migrations` جوه `Library.DAL`. **ماتعملش `Update-Database`** — المشروع بيطبّق الـ Migration لوحده عند التشغيل.
5. اضغط **F5**. أول تشغيل بيعمل: الداتابيز + الجداول + الأدوار + 3 حسابات تجربة + تصنيفات وكتب تجريبية.

### حسابات التجربة (من `appsettings.Development.json`)
| الدور | الإيميل | الباسورد |
|---|---|---|
| Admin | admin@library.local | Admin@12345 |
| Librarian | librarian@library.local | Librarian@12345 |
| Member | member@library.local | Member@12345 |

> ⚠️ دي للتجربة على جهازك بس (موجودة في ملف Development فقط ومش بتشتغل في Production).
> لو هترفع المشروع على GitHub غيّرها واستخدم User Secrets:
> `dotnet user-secrets set "SeedAdmin:Password" "كلمة_قوية" --project Library.PL`

---

## 2) المشروع بيعمل إيه؟

**الزائر (من غير تسجيل):** يتصفح الكتب، يبحث بالعنوان/المؤلف، يفلتر بالتصنيف، يشوف التفاصيل ومدى التوفر.

**العضو (Member):** يسجّل حساب بنفسه (`/account/register`) ثم من صفحته **My Library** (`/my`):
- يطلب استعارة كتاب متاح (الطلب بيفضل Pending لحد ما الموظف يوافق).
- يلغي طلبه وهو لسه Pending.
- يشوف: الكتب اللي معاه، ميعاد الإرجاع، الغرامات، وتاريخه.

**الموظف (Librarian):** منطقة الموظفين (`/admin/books`):
- إدارة الكتب (CRUD كامل) والتصنيفات.
- إدارة الأعضاء (إضافة/تعديل/تفعيل-إيقاف/تفاصيل وسجل استعارات).
- الاستعارات: موافقة/رفض الطلبات، وتسليم كتاب مباشرة من المكتب (Lend).
- الإرجاع (Returns): تأكيد الإرجاع، ومتابعة المتأخرات، وتسجيل دفع الغرامة كاش.

**الأدمن (Admin):** كل اللي فوق + إنشاء حسابات موظفين (`/admin/staff`).
مفيش أي طريقة إن حد يختار لنفسه Admin/Librarian من صفحة التسجيل.

### مطابقة المشروع مع الـ Use Cases (نفس ترتيب الـ Diagrams)
| # | Use Case | Actor | الشاشة / المكان في الكود |
|---|---|---|---|
| 1 | Browse Books | Visitor | `/books` → `BooksController.Index` |
| 2 | Search Books | Visitor | `/books?search=…&categoryId=…` → نفس الـ Action |
| 3 | View Book Details | Visitor | `/books/details/{id}` → `BooksController.Details` |
| 4 | Register | Visitor | `/account/register` → `AccountController.Register` + `MemberService.CreateMemberAsync` |
| 5 | Login | Visitor / Member / Librarian / Admin | `/account/login` → `AccountController.Login` |
| 6 | Request Borrowing | Member | زر **Request to borrow** في صفحة الكتاب → `MyLibraryController.Borrow` → `BorrowingService.RequestBorrow` |
| 7 | Cancel Request | Member | زر **Cancel request** في `/my` → `MyLibraryController.CancelRequest` → `BorrowingService.Cancel` |
| 8 | View My Library | Member | `/my` → `MyLibraryController.Index` |
| 9 | Manage Books | Librarian | `/admin/books` (List / Create / Edit / Delete / Details) → `AdminBooksController` |
| 10 | Manage Categories | Librarian | `/admin/categories` → `AdminCategoriesController` |
| 11 | Manage Members | Librarian | `/admin/members` (List / Create / Edit / Details) → `AdminMembersController` |
| 12 | Manage Borrowing | Librarian | `/admin/borrowings` (Approve / Reject / Lend) → `AdminBorrowingsController` |
| 13 | Manage Returns | Librarian | `/admin/returns` (Confirm return / Fine paid) → `AdminReturnsController` |
| 14 | Manage Staff Accounts | Admin | `/admin/staff` → `AdminStaffController` |

> كل Use Case له Activity Diagram و Sequence Diagram في ملف الـ diagrams، وكل واحد منهم ليه شاشة واحدة واضحة هنا.

### قواعد المكتبة (في `Library.DAL/Entities/LibraryRules.cs` — غيّر الرقم وهيتطبق في كل مكان)
- مدة الاستعارة: **14 يوم**
- غرامة التأخير: **5 جنيه/يوم** (بتتحسب وقت الإرجاع = أيام التأخير × 5)
- أقصى عدد للعضو: **3** (كتب معاه + طلبات معلّقة)
- العضو **ممنوع يستعير** لو: عضويته متوقفة، أو معاه كتاب متأخر، أو عليه غرامة لسه ما اتدفعتش، أو معاه نفس الكتاب بالفعل.

### مسار الاستعارة
```
العضو يطلب  ──► Pending ──الموظف يوافق──► Borrowed ──الموظف يأكد الإرجاع──► Returned
                   │                          │                                  │
                   ├─ العضو يلغي → Cancelled  └─ لو متأخر                        └─ لو فيه غرامة: Unpaid ──دفع كاش──► Paid
                   └─ الموظف يرفض → Rejected
الموظف ممكن يعمل Lend مباشرة (من غير طلب) فيروح على Borrowed علطول.
```
النسخ: `AvailableCopies` بتقل لما الكتاب يخرج (Borrowed) وبتزيد لما يرجع. الطلب المعلّق **مش بيحجز نسخة**؛ النسخة بتتخصم وقت الموافقة.

---

## 3) هيكل المشروع وشرح كل ملف

```
Library.PL   (Presentation)   → Controllers + Views + Program   (الواجهة)
   │ بيعتمد على
Library.BLL  (Business Logic) → Services                         (قواعد الشغل)
   │ بيعتمد على
Library.DAL  (Data Access)    → Entities + Configs + DbContext + Repositories (الداتا)
```
قاعدة مهمة: الـ Controller ماينادي على الـ DbContext مباشرة؛ بينادي على Repository (للـ CRUD البسيط) أو Service (لو فيه قواعد).

### 🗄️ Library.DAL
| الملف | بيعمل إيه |
|---|---|
| `Entities/Book.cs` | الكتاب. `Copies` = الإجمالي، `AvailableCopies` = المتاح دلوقتي. |
| `Entities/Category.cs` | التصنيف (اسمه فريد). |
| `Entities/ApplicationUser.cs` | حساب الدخول (إيميل/باسورد/أدوار). موروث من `IdentityUser`. |
| `Entities/Member.cs` | بيانات العضوية (رقم العضوية، الاسم، التليفون…) ومربوط بالحساب One-to-One. |
| `Entities/Borrowing.cs` | سجل استعارة كتاب واحد: الحالة، التواريخ، الغرامة. فيه خصائص محسوبة (`OverdueDays`, `IsOverdue`, `CurrentFine`) مش بتتخزن في الداتابيز. |
| `Entities/Enums.cs` | حالات: `BorrowingStatus`, `FineStatus`. |
| `Entities/LibraryRules.cs` | ثوابت القواعد (14 يوم، 5 جنيه، 3 كتب). |
| `Configurations/*Config.cs` | إعدادات Fluent API لكل جدول (الأطوال، الـ Unique، العلاقات، `Restrict` عشان ماينفعش تمسح كتاب/عضو عليه تاريخ). |
| `Contexts/AppDbContext.cs` | بيورث من `IdentityDbContext` (فيه جداول Identity + جداولنا). السطر `base.OnModelCreating` مهم جداً. |
| `Repositories/…/GenericRepository.cs` | GetAll / GetById / Add / Update / Delete / Save لأي Entity. |
| `Repositories/…/Book/Category/Member/Borrowing Repository.cs` | كل واحد بيزوّد استعلامات خاصة بيه (زي `GetAllDetailed` بالـ `Include`، و`HasOverdue`، و`GetUnpaidFines`). |

### ⚙️ Library.BLL
| الملف | بيعمل إيه |
|---|---|
| `Common/ServiceResult.cs` | نتيجة أي عملية: نجحت؟ + رسالة. الـ Controller بيعرض الرسالة. |
| `Services/…/MemberService.cs` | بيعمل حساب + Role Member + سجل Member مع بعض (وبيمسح الحساب لو حصل خطأ). بحث وتعديل الأعضاء. |
| `Services/…/BorrowingService.cs` | **قلب النظام**: طلب/موافقة/رفض/إلغاء/Lend/إرجاع/دفع غرامة + كل القواعد. |

### 🖥️ Library.PL
| الملف | بيعمل إيه |
|---|---|
| `Program.cs` | تسجيل الـ Services والـ Repositories (DI)، إعداد Identity (باسورد 8+ مع رقم، Lockout بعد 5 محاولات)، الـ Cookie، حماية Anti-Forgery لكل POST، وتطبيق الـ Migrations وتشغيل الـ Seeder عند البداية. |
| `Seed/IdentitySeeder.cs` | بيعمل الأدوار والحسابات والبيانات التجريبية (آمن لو اتشغل أكتر من مرة). |
| `Controllers/HomeController, BooksController` | الصفحة الرئيسية وتصفح الكتب (عام). |
| `Controllers/AccountController` | Login / Register / Logout / AccessDenied. |
| `Controllers/MyLibraryController` | صفحة العضو وطلب الاستعارة وإلغاء الطلب (`[Authorize(Roles="Member")]`). |
| `Controllers/Admin*Controller` | لوحة التحكم (`[Authorize(Roles="Admin,Librarian")]`)، وإدارة الحسابات للـ Admin بس. |
| `ViewModels/*` | كلاسات خاصة بالفورمز (Login, Register, Lend, MemberEdit, …). |
| `Views/Shared/_Layout` و `_AdminLayout` | تصميم الموقع العام وتصميم لوحة الإدارة. |
| `Views/Shared/_*Partial` | أجزاء متكررة: الكارت، شارة الحالة، شارة الغرامة، حقول التسجيل، رسائل النجاح/الخطأ. |

### الروابط
`/` · `/books` · `/books/details/{id}` · `/account/login` · `/account/register` · `/my` ·
`/admin/books` · `/admin/categories` · `/admin/members` · `/admin/borrowings` · `/admin/borrowings/lend` · `/admin/returns` · `/admin/staff`

---

## 4) تجربة سريعة تشوف بيها كل حاجة
1. ادخل بـ **member** → افتح كتاب → **Request to borrow** → هتلاقيه Pending في `/my`.
2. اطلع وادخل بـ **librarian** → `/admin/borrowings?status=pending` → **Approve**.
3. ارجع للعضو: الكتاب بقى Borrowed وفيه ميعاد إرجاع.
4. من الموظف: **Confirm return** → الكتاب رجع والنسخة اتزادت.
5. لتجربة الغرامة: حط `LoanDays = 0` في `LibraryRules.cs` مؤقتاً، استعير، استنى للتاني يوم (أو عدّل `DueDate` في الداتابيز لتاريخ قديم)، وبعدين Return → هتظهر غرامة Unpaid وبعدها **Fine paid (cash)**.

---

## 5) ملاحظات وحدود (عن قصد، عشان نفضل بسيطين)
- مفيش دفع أونلاين/إشعارات/باركود (حسب الاتفاق). الغرامة بتتسجل والموظف بيأكد الدفع كاش.
- المشروع فيه الـ 14 Use Case بس (زي الـ Diagrams): **مفيش** حجز مسبق، ولا Dashboard/تقارير إحصائيات، ولا أي شاشة مش مربوطة بـ Use Case. أي وظيفة مش موجودة في الـ Diagrams بتتشال.
- مفيش حذف للأعضاء (بنوقف العضوية بدل الحذف عشان نحافظ على التاريخ). والكتاب اللي عليه تاريخ مايتمسحش.
- التواريخ بتستخدم `DateTime.Now` (توقيت جهاز السيرفر) لأن المكتبة في مكان واحد.
- الـ Repositories والـ Services (غير Identity) سنكرونس عشان البساطة.
- مفيش "نسيت الباسورد"/تأكيد إيميل.

## 6) أخطاء شائعة
| الرسالة | الحل |
|---|---|
| `مفيش Migrations في المشروع...` | نفّذ خطوة 4 (`Add-Migration`). |
| `There is already an object named 'Categories'` | الداتابيز القديمة لسه موجودة → خطوة 1 (Drop-Database). |
| `Cannot open database "LibraryDb"` / مش لاقي السيرفر | اسم السيرفر غلط في `appsettings.json` (جرّب `(localdb)\MSSQLLocalDB`). |
| `NETSDK1045 ... does not support targeting .NET 10` | نزّل .NET 10 SDK أو ارجع لـ net9.0 (فوق). |
| Add-Migration: "target project doesn't match migrations assembly" | استخدم الأمر بالضبط زي ما هو (فيه `-Project Library.DAL`). |
