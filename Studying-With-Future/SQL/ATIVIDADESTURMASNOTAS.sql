SELECT 
    ativ.Id AS AtividadeId,
    ativ.Titulo,
    ativ.Descricao,
    ativ.DataEntrega,
    u.Nome AS Aluno,
    n.Valor AS Nota,
    n.Observacao
FROM Atividades ativ
LEFT JOIN Notas n ON ativ.Id = n.AtividadeId
LEFT JOIN Usuarios u ON n.AlunoId = u.Id AND u.TipoUsuario = 'Aluno'
WHERE ativ.TurmaId = [ID_DA_TURMA];
