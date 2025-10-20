SELECT 
    u.Id,
    u.Nome,
    u.Matricula,
    AVG(n.Valor) AS MediaGeral
FROM Usuarios u
INNER JOIN AlunoTurmas at ON u.Id = at.AlunoId
INNER JOIN Turmas t ON at.TurmaId = t.Id
INNER JOIN Atividades ativ ON t.Id = ativ.TurmaId
INNER JOIN Notas n ON ativ.Id = n.AtividadeId AND n.AlunoId = u.Id
WHERE u.TipoUsuario = 'Aluno'
  AND t.Id = [ID_DA_TURMA]
GROUP BY u.Id, u.Nome, u.Matricula;
