-- View all Students
SELECT * FROM Students;

-- View all Courses
SELECT * FROM Courses;

-- View all Rooms
SELECT * FROM Rooms;

-- View all Exam Seatings with related data
SELECT 
    es.*,
    s.Name as StudentName,
    s.RollNumber,
    c.Code as CourseCode,
    c.Name as CourseName,
    r.RoomNumber
FROM ExamSeatings es
JOIN Students s ON es.StudentId = s.Id
JOIN Courses c ON es.CourseId = c.Id
JOIN Rooms r ON es.RoomId = r.Id; 