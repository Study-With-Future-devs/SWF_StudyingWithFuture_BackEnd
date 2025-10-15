SELECT 
    a.Id,
    a.Nome,
    a.Email,
    a.Matricula,
    a.Periodo
FROM Alunos a
INNER JOIN AlunoTurmas at ON a.Id = at.AlunoId
WHERE at.TurmaId = [ID_DA_TURMA];