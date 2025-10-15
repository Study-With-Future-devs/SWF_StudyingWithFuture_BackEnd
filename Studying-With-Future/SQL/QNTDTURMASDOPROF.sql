SELECT 
    p.Id,
    p.Nome,
    p.Formacao,
    p.Especialidade,
    COUNT(t.Id) as QuantidadeTurmas
FROM Professores p
LEFT JOIN Turmas t ON p.Id = t.ProfessorId
GROUP BY p.Id, p.Nome, p.Formacao, p.Especialidade;