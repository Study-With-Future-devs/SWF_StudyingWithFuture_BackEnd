SELECT 
    u.Id,
    u.Nome,
    u.Email,
    u.Matricula,
    u.Periodo,
    (SELECT AVG(n.Valor)
     FROM Notas n
     INNER JOIN Atividades ativ ON n.AtividadeId = ativ.Id
     INNER JOIN AlunoTurmas at ON at.TurmaId = ativ.TurmaId AND at.AlunoId = n.AlunoId
     WHERE n.AlunoId = u.Id) AS MediaGeral
FROM Usuarios u
WHERE u.TipoUsuario = 'Aluno';
