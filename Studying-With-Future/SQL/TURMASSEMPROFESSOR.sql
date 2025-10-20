SELECT 
    t.Id,
    t.Codigo,
    t.Descricao
FROM Turmas t
WHERE t.ProfessorId IS NULL;
