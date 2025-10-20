SELECT 
    t.Id,
    t.Codigo,
    t.Descricao
FROM Turmas t
WHERE t.ProfessorId = [ID_DO_PROFESSOR];
