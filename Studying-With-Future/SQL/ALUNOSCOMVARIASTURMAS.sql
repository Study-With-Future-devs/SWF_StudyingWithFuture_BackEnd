SELECT 
    a.Id,
    a.Nome,
    a.Matricula,
    COUNT(at.TurmaId) as QuantidadeTurmas
FROM Alunos a
INNER JOIN AlunoTurmas at ON a.Id = at.AlunoId
GROUP BY a.Id, a.Nome, a.Matricula
HAVING COUNT(at.TurmaId) > 1;