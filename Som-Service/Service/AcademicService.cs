using Dapper;
using Som_Models.Models;
using Som_Service.Interface;

namespace Som_Service.Service
{
    public class AcademicService : IAcademicService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AcademicService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Curriculum>> GetCurriculumsAsync()
        {
            await SeedAcademicDataAsync();
            using var db = _connectionFactory.CreateConnection();

            var list = await db.QueryAsync<Curriculum>("SELECT Id, Name, Description FROM Curriculums");
            return list.AsList();
        }

        public async Task<List<string>> GetClassesAsync(string curriculumType)
        {
            await SeedAcademicDataAsync();
            using var db = _connectionFactory.CreateConnection();

            var sql = "SELECT DISTINCT ClassName FROM CurriculumSubjects WHERE CurriculumType = @CurriculumType";
            var list = await db.QueryAsync<string>(sql, new { CurriculumType = curriculumType });
            return list.AsList();
        }

        public async Task<List<string>> GetSubjectsAsync(string curriculumType, string className)
        {
            await SeedAcademicDataAsync();
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT SubjectName FROM CurriculumSubjects 
                WHERE CurriculumType = @CurriculumType AND ClassName = @ClassName";

            var list = await db.QueryAsync<string>(sql, new { CurriculumType = curriculumType, ClassName = className });
            return list.AsList();
        }

        public async Task SeedAcademicDataAsync()
        {
            using var db = _connectionFactory.CreateConnection();

            var checkTable = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM sysobjects WHERE name='CurriculumSubjects' AND xtype='U'");

            if (checkTable == 0)
            {
                var createScript = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Curriculums' AND xtype='U')
                    BEGIN
                        CREATE TABLE Curriculums (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(100) NOT NULL,
                            Description NVARCHAR(255) NULL
                        );
                        INSERT INTO Curriculums (Name, Description) VALUES 
                        (N'General', N'Bangladesh National Curriculum (NCTB - Bangla & English Version)'),
                        (N'English Medium', N'International Curriculum (Cambridge / Pearson Edexcel)');
                    END

                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CurriculumSubjects' AND xtype='U')
                    BEGIN
                        CREATE TABLE CurriculumSubjects (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            CurriculumType NVARCHAR(100) NOT NULL,
                            ClassName NVARCHAR(100) NOT NULL,
                            SubjectName NVARCHAR(100) NOT NULL
                        );

                        INSERT INTO CurriculumSubjects (CurriculumType, ClassName, SubjectName) VALUES
                        ('General', 'Class 1', 'Bangla'), ('General', 'Class 1', 'English'), ('General', 'Class 1', 'Mathematics'),
                        ('General', 'Class 2', 'Bangla'), ('General', 'Class 2', 'English'), ('General', 'Class 2', 'Mathematics'),
                        ('General', 'Class 3', 'Bangla'), ('General', 'Class 3', 'English'), ('General', 'Class 3', 'Mathematics'), ('General', 'Class 3', 'Science'),
                        ('General', 'Class 4', 'Bangla'), ('General', 'Class 4', 'English'), ('General', 'Class 4', 'Mathematics'), ('General', 'Class 4', 'Science'),
                        ('General', 'Class 5', 'Bangla'), ('General', 'Class 5', 'English'), ('General', 'Class 5', 'Mathematics'), ('General', 'Class 5', 'Science'),
                        ('General', 'Class 6', 'Bangla'), ('General', 'Class 6', 'English'), ('General', 'Class 6', 'Mathematics'), ('General', 'Class 6', 'Science'), ('General', 'Class 6', 'BGS'), ('General', 'Class 6', 'ICT'),
                        ('General', 'Class 7', 'Bangla'), ('General', 'Class 7', 'English'), ('General', 'Class 7', 'Mathematics'), ('General', 'Class 7', 'Science'), ('General', 'Class 7', 'BGS'), ('General', 'Class 7', 'ICT'),
                        ('General', 'Class 8', 'Bangla'), ('General', 'Class 8', 'English'), ('General', 'Class 8', 'Mathematics'), ('General', 'Class 8', 'Science'), ('General', 'Class 8', 'BGS'), ('General', 'Class 8', 'ICT'),
                        ('General', 'Class 9', 'Bangla'), ('General', 'Class 9', 'English'), ('General', 'Class 9', 'General Mathematics'), ('General', 'Class 9', 'Higher Mathematics'), ('General', 'Class 9', 'Physics'), ('General', 'Class 9', 'Chemistry'), ('General', 'Class 9', 'Biology'), ('General', 'Class 9', 'Accounting'), ('General', 'Class 9', 'ICT'),
                        ('General', 'Class 10', 'Bangla'), ('General', 'Class 10', 'English'), ('General', 'Class 10', 'General Mathematics'), ('General', 'Class 10', 'Higher Mathematics'), ('General', 'Class 10', 'Physics'), ('General', 'Class 10', 'Chemistry'), ('General', 'Class 10', 'Biology'), ('General', 'Class 10', 'Accounting'), ('General', 'Class 10', 'ICT');

                        INSERT INTO CurriculumSubjects (CurriculumType, ClassName, SubjectName) VALUES
                        ('English Medium', 'Grade 1', 'English'), ('English Medium', 'Grade 1', 'Mathematics'), ('English Medium', 'Grade 1', 'Science'),
                        ('English Medium', 'Grade 2', 'English'), ('English Medium', 'Grade 2', 'Mathematics'), ('English Medium', 'Grade 2', 'Science'),
                        ('English Medium', 'Grade 3', 'English'), ('English Medium', 'Grade 3', 'Mathematics'), ('English Medium', 'Grade 3', 'Science'),
                        ('English Medium', 'Grade 4', 'English'), ('English Medium', 'Grade 4', 'Mathematics'), ('English Medium', 'Grade 4', 'Science'),
                        ('English Medium', 'Grade 5', 'English'), ('English Medium', 'Grade 5', 'Mathematics'), ('English Medium', 'Grade 5', 'Science'),
                        ('English Medium', 'Grade 6', 'English Language'), ('English Medium', 'Grade 6', 'Mathematics'), ('English Medium', 'Grade 6', 'General Science'), ('English Medium', 'Grade 6', 'ICT'),
                        ('English Medium', 'Grade 7', 'English Language'), ('English Medium', 'Grade 7', 'Mathematics'), ('English Medium', 'Grade 7', 'General Science'), ('English Medium', 'Grade 7', 'ICT'),
                        ('English Medium', 'Grade 8', 'English Language'), ('English Medium', 'Grade 8', 'Mathematics'), ('English Medium', 'Grade 8', 'General Science'), ('English Medium', 'Grade 8', 'ICT'),
                        ('English Medium', 'O-Level (Grade 9)', 'English Language'), ('English Medium', 'O-Level (Grade 9)', 'Pure Mathematics'), ('English Medium', 'O-Level (Grade 9)', 'Physics'), ('English Medium', 'O-Level (Grade 9)', 'Chemistry'), ('English Medium', 'O-Level (Grade 9)', 'Biology'), ('English Medium', 'O-Level (Grade 9)', 'Economics'),
                        ('English Medium', 'O-Level (Grade 10)', 'English Language'), ('English Medium', 'O-Level (Grade 10)', 'Pure Mathematics'), ('English Medium', 'O-Level (Grade 10)', 'Physics'), ('English Medium', 'O-Level (Grade 10)', 'Chemistry'), ('English Medium', 'O-Level (Grade 10)', 'Biology'), ('English Medium', 'O-Level (Grade 10)', 'Economics');
                    END";

                await db.ExecuteAsync(createScript);
            }
        }
    }
}
