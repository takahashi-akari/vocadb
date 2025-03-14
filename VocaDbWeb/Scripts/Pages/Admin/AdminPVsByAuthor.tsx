import SafeAnchor from '@/Bootstrap/SafeAnchor';
import { SongLink } from '@/Components/Shared/Partials/Song/SongLink';
import { PVForSongContract } from '@/DataContracts/PVForSongContract';
import { useLoginManager } from '@/LoginManagerContext';
import { adminRepo } from '@/Repositories/AdminRepository';
import React from 'react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';

const AdminPVsByAuthor = () => {
	const [pvs, setPVs] = useState<PVForSongContract[]>([]);
	const [author, setAuthor] = useState('');
	const loginManager = useLoginManager();

	const onSubmit = (): void => {
		adminRepo.getPVsByAuthor(author).then((resp) => setPVs(resp));
	};

	return (
		<React.Fragment>
			<form
				onSubmit={(e): void => {
					e.preventDefault();
					onSubmit();
				}}
			>
				<input
					onChange={(e): void => setAuthor(e.target.value)}
					id="author"
					name="Author"
					type="text"
					value={author}
				></input>
				<button type="submit" className="btn btn-primary">
					Apply
				</button>
			</form>
			{pvs.length > 0 && (
				<React.Fragment>
					<p>{`${pvs.length} PVs found`}</p>
					<table>
						{pvs.map((pv, index) => (
							<tr>
								<td>
									<a href={pv.url}>
										<img src={pv.thumbUrl} alt={pv.name} />
									</a>
								</td>
								<td>
									<a href={pv.url}>{pv.name}</a>
									<br />
									<td>
										<SongLink song={pv.song} />
									</td>
								</td>
							</tr>
						))}
					</table>
					{loginManager.canBulkDeletePVs && (
						<React.Fragment>
							<br />
							<SafeAnchor
								className="btn btn-danger"
								onClick={(): void => {
									if (
										window.confirm(
											`${pvs.length} PVs will be deleted. Are you sure?`,
										)
									) {
										adminRepo
											.deletePVsByAuthor(author)
											.then((): void => onSubmit());
									}
								}}
							>
								Delete PVs by this author
							</SafeAnchor>
						</React.Fragment>
					)}
				</React.Fragment>
			)}
		</React.Fragment>
	);
};

export default AdminPVsByAuthor;
