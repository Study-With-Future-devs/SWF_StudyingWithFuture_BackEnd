SELECT 
    a.Id,
    a.Nome,
    a.Matricula,
    AVG(n.Valor) as MediaGeral
FROM Alunos a
INNER JOIN AlunoTurmas at ON a.Id = at.AlunoId
INNER JOIN Turmas t ON at.TurmaId = t.Id
INNER JOIN Atividades ativ ON t.Id = ativ.TurmaId
INNER JOIN Notas n ON ativ.Id = n.AtividadeId AND n.AlunoId = a.Id
WHERE t.Id = [ID_DA_TURMA]
GROUP BY a.Id, a.Nome, a.Matricula;